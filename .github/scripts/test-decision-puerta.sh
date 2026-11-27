#!/usr/bin/env bash
# Ejecuta el paso "Decidir si se fusiona" de auto-merge.yml (el de la rama, tal cual) contra PRs falsos:
# un gh falso responde a sus llamadas a la API con los datos de cada caso. El caso bueno tiene que fusionarse
# y cada caso malo no, y por su motivo. Así el filtro de ficheros y la comprobación de la versión se prueban
# en cada ejecución de la ci, y no sólo que sus líneas siguen en el fichero.
# Uso: test-decision-puerta.sh <carpeta de workflows>. Sale con 1 si algo falla.
set -uo pipefail
dir="${1:?carpeta de workflows}"
am="$dir/auto-merge.yml"
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
mal=0
command -v jq > /dev/null || { echo "::error::Falta jq."; exit 2; }

# El script del paso: las líneas de su "run: |", sin los 10 espacios de sangría.
awk '
  /^      - name: Decidir si se fusiona[[:space:]]*$/ { paso = 1; next }
  paso && /^        run: \|/ { dentro = 1; next }
  dentro && /^      - / { exit }
  dentro { sub(/^          /, ""); print }' "$am" > "$tmp/decidir.sh"
[ -s "$tmp/decidir.sh" ] || { echo "::error::No encuentro el paso 'Decidir si se fusiona' en auto-merge.yml."; exit 2; }
jobs_en_verde=$(grep -oP '^      JOBS_EN_VERDE:\s*\K.*' "$am")
[ -n "$jobs_en_verde" ] || { echo "::error::No encuentro JOBS_EN_VERDE en auto-merge.yml."; exit 2; }

# gh falso: "api [-X M] RUTA [-f k=v]... [--paginate] [--jq EXPR]" responde con el fichero de datos que toca;
# "pr comment N ... --body TEXTO" lo apunta en comentarios.txt.
mkdir -p "$tmp/bin"
cat > "$tmp/bin/gh" << 'FALSO'
#!/usr/bin/env bash
set -uo pipefail
d="$DATOS"
if [ "$1" = "pr" ] && [ "$2" = "comment" ]; then
  while [ $# -gt 0 ]; do [ "$1" = "--body" ] && { printf '%s\n---\n' "$2" >> "$d/comentarios.txt"; }; shift; done
  exit 0
fi
[ "$1" = "api" ] || { echo "gh falso: no sé hacer '$*'" >&2; exit 99; }
shift
ruta=""; expr="."
while [ $# -gt 0 ]; do
  case "$1" in
    -X|-f|-F) shift 2 ;;
    --paginate) shift ;;
    --jq) expr="$2"; shift 2 ;;
    *) ruta="$1"; shift ;;
  esac
done
ruta="${ruta%%\?*}"
case "$ruta" in
  repos/*/pulls) f=lista.json ;;
  repos/*/pulls/*/files) f=ficheros.json ;;
  repos/*/pulls/*) f=pr.json ;;
  repos/*/actions/runs/*/jobs) f=jobs.json ;;
  repos/*/issues/*/comments) f=comentarios.json ;;
  *) echo "gh falso: ruta inesperada '$ruta'" >&2; exit 99 ;;
esac
jq -r "$expr" "$d/$f"
FALSO
chmod +x "$tmp/bin/gh"

# Datos del caso bueno: un PR de auto-pr.yml, ci en verde, cambia código y sube la menor.
base() {
  local d="$1"; mkdir -p "$d"
  jq -n '{number: 7, head: {repo: {full_name: "kitpymes/prueba"}, sha: "abc123"}, user: {login: "github-actions[bot]"},
          draft: false, labels: [], changed_files: 2}' > "$d/pr.json"
  jq -s '.' "$d/pr.json" > "$d/lista.json"
  jq -n --arg j "$jobs_en_verde" '{jobs: ($j | split(" ") | map(select(. != "") | {name: ., conclusion: "success"}))}' > "$d/jobs.json"
  jq -n '[
    {filename: "src/Clase.cs", status: "modified", patch: "@@ -1 +1 @@\n-a\n+b"},
    {filename: "docs/props/Pack.Build.props", status: "modified",
     patch: "@@ -5,3 +5,3 @@\n     <PropertyGroup>\n-        <Version>2.0.1</Version>\n+        <Version>2.1.0</Version>\n     </PropertyGroup>"}
  ]' > "$d/ficheros.json"
  echo '[]' > "$d/comentarios.json"
}
# Añade un fichero al PR (y sube changed_files).
anade() { local d="$1"; jq --argjson f "$2" '. + [$f]' "$d/ficheros.json" > "$d/x" && mv "$d/x" "$d/ficheros.json"
          jq '.changed_files += 1' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"; }
# Cambia el diff de Pack.Build.props.
props() { local d="$1"; jq --arg p "$(printf '%b' "$2")" 'map(if .filename == "docs/props/Pack.Build.props" then .patch = $p else . end)' "$d/ficheros.json" > "$d/x" && mv "$d/x" "$d/ficheros.json"; }

ejecutar() {   # ejecutar <carpeta de datos>: deja la salida en salida.txt y lo de GITHUB_OUTPUT en salida.out
  local d="$1"; : > "$d/salida.out"
  ( cd "$d" && DATOS="$d" PATH="$tmp/bin:$PATH" REPO=kitpymes/prueba RAMA=rama SHA_CI=abc123 RUN_CI=1 \
      RESULTADO_CI="${RESULTADO:-success}" JOBS_EN_VERDE="$jobs_en_verde" GITHUB_OUTPUT="$d/salida.out" RUNNER_TEMP="$d" \
      bash "$tmp/decidir.sh" ) > "$d/salida.txt" 2>&1
  echo $? > "$d/codigo"
}

caso() {   # caso <nombre> <fusiona: true|false> <texto que tiene que salir> [comentario esperado]  (datos en $d)
  ejecutar "$d"
  local fusiona; fusiona=$(grep -oP '^fusionar=\K.*' "$d/salida.out" | tail -n 1)
  if [ "$(cat "$d/codigo")" != 0 ] || [ "$fusiona" != "$2" ]; then
    cat "$d/salida.txt"; echo "::error::Caso '$1': salida $(cat "$d/codigo"), fusionar='$fusiona' (se esperaba '$2')."; mal=1
  elif ! grep -qF -- "$3" "$d/salida.txt"; then
    cat "$d/salida.txt"; echo "::error::Caso '$1': fusionar=$2, pero no por '$3'."; mal=1
  elif [ -n "${4:-}" ] && ! grep -qF -- "$4" "$d/comentarios.txt" 2>/dev/null; then
    echo "::error::Caso '$1': no deja el comentario '$4'."; mal=1
  else
    echo "Caso '$1': fusionar=$2 por '$3'."
  fi
}

n=0; nuevo() { n=$((n + 1)); d="$tmp/caso$n"; base "$d"; }

nuevo; caso 'PR normal, sube la menor' true 'Versión 2.0.1 → 2.1.0'
nuevo; props "$d" '@@ -1 +1 @@\n     <Authors>x</Authors>'; caso 'PR sin cambio de versión' true 'La versión no cambia'
nuevo; anade "$d" '{"filename": ".github/workflows/auto-merge.yml", "status": "modified", "patch": "x"}'
       caso 'toca la puerta' false 'toca .github/** o nuget.config (.github/workflows/auto-merge.yml' '<!-- auto-merge:puerta -->'
nuevo; anade "$d" '{"filename": ".github/scripts/check-mutacion.py", "status": "added", "patch": "x"}'
       caso 'añade un script de la ci' false 'toca .github/** o nuget.config'
nuevo; anade "$d" '{"filename": "docs/mover.yml", "previous_filename": ".github/workflows/ci.yml", "status": "renamed", "patch": "x"}'
       caso 'saca un workflow de .github' false 'toca .github/** o nuget.config (docs/mover.yml'
nuevo; anade "$d" '{"filename": "NuGet.Config", "status": "added", "patch": "x"}'
       caso 'añade NuGet.Config' false 'toca .github/** o nuget.config (NuGet.Config'
nuevo; jq '.changed_files = 3' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"
       caso 'lista de ficheros incompleta' false 'no se pudo leer la lista completa de ficheros'
nuevo; props "$d" '@@ -5,3 +5,3 @@\n-        <Version>2.1.0</Version>\n+        <Version>3.0.0</Version>'
       caso 'sube la mayor' false 'cambia la versión mayor (2.1.0 → 3.0.0)' '<!-- auto-merge:version-mayor -->'
nuevo; props "$d" '@@ -5,3 +5,3 @@\n-        <Version>2.1.0</Version>\n+        <Version>1.9.0</Version>'
       caso 'baja la mayor' false 'cambia la versión mayor (2.1.0 → 1.9.0)'
nuevo; props "$d" '@@ -5,2 +5,1 @@\n-        <Version>2.1.0</Version>'
       caso 'quita la versión' false 'no tiene exactamente una <Version> quitada y una puesta'
nuevo; props "$d" '@@ -5,2 +5,3 @@\n-        <Version>2.1.0</Version>\n+        <Version>2.2.0</Version>\n+        <Version>3.0.0</Version>'
       caso 'dos versiones puestas' false 'no tiene exactamente una <Version> quitada y una puesta'
nuevo; props "$d" '@@ -5,3 +5,3 @@\n-        <Version>2.0.1</Version>\n+        <Version>2.1.0</Version>\n-        <PackageValidationBaselineVersion>2.0.0</PackageValidationBaselineVersion>\n+        <PackageValidationBaselineVersion>2.0.1</PackageValidationBaselineVersion>'
       caso 'versión y versión de referencia' true 'Versión 2.0.1 → 2.1.0'
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n-    <PackageReference Include=\"Kitpymes.Core.Shared\" Version=\"2.0.1\" />\r\n+    <PackageReference Include=\"Kitpymes.Core.Shared\" Version=\"2.1.0\" />\r"}'
       caso 'sube un PackageReference' true 'Versión 2.0.1 → 2.1.0'
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n-    <PackageReference Include=\"StyleCop.Analyzers\" Version=\"1.1.118\" PrivateAssets=\"All\" IncludeAssets=\"Analyzers;Build\" />\n+    <PackageReference Include=\"StyleCop.Analyzers\" Version=\"1.2.0\" PrivateAssets=\"All\" IncludeAssets=\"Analyzers;Build\" />"}'
       caso 'sube un analizador con PrivateAssets' true 'Versión 2.0.1 → 2.1.0'
# Lo que MSBuild ejecuta en master: cualquier cambio fuera de la versión lo fusiona Sebastián.
M='cambia MSBuild más allá de la versión'
nuevo; jq 'map(if .filename == "docs/props/Pack.Build.props" then .patch = null else . end)' "$d/ficheros.json" > "$d/x" && mv "$d/x" "$d/ficheros.json"
       caso 'props sin diff' false "$M (docs/props/Pack.Build.props"
nuevo; props "$d" '@@ -5,2 +5,3 @@\n+        <Version Condition="true">3.0.0</Version>'
       caso 'versión con condición' false "$M (docs/props/Pack.Build.props" '<!-- auto-merge:msbuild -->'
nuevo; props "$d" '@@ -5,2 +5,3 @@\n+        <PackageVersion>3.0.0</PackageVersion>'
       caso 'PackageVersion' false "$M (docs/props/Pack.Build.props"
nuevo; props "$d" '@@ -5,3 +5,3 @@\n-        <Version>2.0.1</Version>\n+        <Version >3.0.0</Version>'
       caso 'versión con espacio' false "$M (docs/props/Pack.Build.props"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n+    <version>9.0.0</version>"}'
       caso 'versión en minúsculas en un csproj' false "$M (src/Lib/Lib.csproj"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n+    <VersionPrefix>9.0.0</VersionPrefix>"}'
       caso 'VersionPrefix en un csproj' false "$M (src/Lib/Lib.csproj"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n+    <PackageReference Include=\"Nuevo.Paquete\" Version=\"1.0.0\" />"}'
       caso 'paquete nuevo' false "$M (src/Lib/Lib.csproj"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.CSPROJ", "status": "modified", "patch": null}'
       caso 'csproj en mayúsculas sin diff' false "$M (src/Lib/Lib.CSPROJ"
nuevo; anade "$d" '{"filename": "Directory.Build.rsp", "status": "added", "patch": "@@ -0,0 +1 @@\n+-p:Version=3.0.0"}'
       caso 'Directory.Build.rsp' false "$M (Directory.Build.rsp"
nuevo; anade "$d" '{"filename": "Directory.Build.targets", "status": "added", "patch": "@@ -0,0 +1 @@\n+<Project><Target Name=\"x\" BeforeTargets=\"Pack\"><Exec Command=\"echo\" /></Target></Project>"}'
       caso 'Directory.Build.targets nuevo' false "$M (Directory.Build.targets"
nuevo; anade "$d" '{"filename": "Directory.Build.props", "status": "modified", "patch": "@@ -1 +1 @@\n+  <RestoreAdditionalProjectSources>https://x.test</RestoreAdditionalProjectSources>"}'
       caso 'otro origen de paquetes' false "$M (Directory.Build.props"
nuevo; anade "$d" '{"filename": "global.json", "status": "modified", "patch": "@@ -1 +1 @@\n-\"version\": \"10.0.100\"\n+\"version\": \"10.0.200\""}'
       caso 'global.json' false "$M (global.json"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj.user", "status": "added", "patch": "@@ -0,0 +1 @@\n+<Project><PropertyGroup><Version>9.0.0</Version></PropertyGroup></Project>"}'
       caso 'csproj.user' false "$M (src/Lib/Lib.csproj.user"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n-    <PackageReference Include=\"A\" Version=\"1.0.0\" />\n+    <PackageReference Include=\"A\" Version=\"$(TOKEN)\" />"}'
       caso 'versión con $()' false "$M (src/Lib/Lib.csproj"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1 +1 @@\n-    <PackageReference Include=\"A\" Version=\"1.0.0\" PrivateAssets=\"All\" />\n+    <PackageReference Include=\"A\" Version=\"1.1.0\" />"}'
       caso 'quita PrivateAssets' false "$M (src/Lib/Lib.csproj"
nuevo; anade "$d" '{"filename": "src/Lib/Lib.csproj", "status": "modified", "patch": "@@ -1,2 +1,2 @@\n-    <PackageReference Include=\"A\" Version=\"1.0.0\" />\n-    <PackageReference Include=\"B\" Version=\"1.0.0\" />\n+    <PackageReference Include=\"A\" Version=\"1.1.0\" />\n+    <PackageReference Include=\"A\" Version=\"1.2.0\" />"}'
       caso 'cambia B por A' false "$M (src/Lib/Lib.csproj"
nuevo; props "$d" '@@ -5,3 +5,2 @@\n-        <Version>2.0.1</Version>\n+        <Version>2.1.0</Version>\n-        <PackageValidationBaselineVersion>2.0.0</PackageValidationBaselineVersion>'
       caso 'quita la versión de referencia' false "$M (docs/props/Pack.Build.props"
nuevo; jq 'map(if .filename == "docs/props/Pack.Build.props" then .status = "renamed" | .previous_filename = "docs/props/Viejo.props" else . end)' "$d/ficheros.json" > "$d/x" && mv "$d/x" "$d/ficheros.json"
       caso 'props creado por renombrado' false "$M (docs/props/Pack.Build.props"
nuevo; jq '.jobs += [{name: "malo\n::set-output name=fusionar::true", conclusion: "failure"}]' "$d/jobs.json" > "$d/x" && mv "$d/x" "$d/jobs.json"
       caso 'nombre de job con un comando' false 'jobs de la ci sin verde: malo  set-output'
nuevo; jq '.user.login = "alguien"' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"; jq -s '.' "$d/pr.json" > "$d/lista.json"
       caso 'PR que no abrió auto-pr' false 'no lo abrió auto-pr.yml'
nuevo; jq '.head.repo.full_name = "otro/fork"' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"; jq -s '.' "$d/pr.json" > "$d/lista.json"
       caso 'rama de un fork' false 'no es de este repo'
nuevo; jq '.head.sha = "otro"' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"; jq -s '.' "$d/pr.json" > "$d/lista.json"
       caso 'commit más nuevo' false 'tiene commits más nuevos'
nuevo; jq '.labels = [{name: "espera-sebastian"}]' "$d/pr.json" > "$d/x" && mv "$d/x" "$d/pr.json"; jq -s '.' "$d/pr.json" > "$d/lista.json"
       caso 'etiqueta espera-sebastian' false 'tiene la etiqueta espera-sebastian'
nuevo; RESULTADO=failure caso 'ci en rojo' false "terminó en 'failure'"
nuevo; primero=${jobs_en_verde%% *}; jq --arg j "$primero" '.jobs |= map(if .name == $j then .conclusion = "skipped" else . end)' "$d/jobs.json" > "$d/x" && mv "$d/x" "$d/jobs.json"
       caso 'job obligatorio omitido' false "el job $primero no está en verde"
nuevo; echo '[]' > "$d/lista.json"; caso 'sin PR' false 'no tiene un PR abierto'

# El comentario no se repite si ya está.
nuevo; anade "$d" '{"filename": ".github/workflows/ci.yml", "status": "modified", "patch": "x"}'
       echo '[{"body": "<!-- auto-merge:puerta -->\nya avisado"}]' > "$d/comentarios.json"
       caso 'aviso repetido' false 'toca .github/** o nuget.config'
       if [ -s "$d/comentarios.txt" ]; then echo "::error::Caso 'aviso repetido': vuelve a comentar."; mal=1; else echo "Caso 'aviso repetido': no vuelve a comentar."; fi

# La prueba tiene que caer si la puerta se rompe: con el filtro de ficheros o la comprobación de la mayor
# desactivados en una copia, esta misma prueba tiene que fallar, y por esos casos.
if [ -z "${PRUEBA_DE_LA_PRUEBA:-}" ]; then
  romper() {   # romper <nombre> <sed> <caso que tiene que fallar>
    rm -rf "$tmp/roto"; cp -r "$dir" "$tmp/roto"; sed -i "$2" "$tmp/roto/auto-merge.yml"
    if cmp -s "$am" "$tmp/roto/auto-merge.yml"; then echo "::error::Romper '$1': el sed no cambió nada."; mal=1; return; fi
    if PRUEBA_DE_LA_PRUEBA=1 bash "$0" "$tmp/roto" > "$tmp/roto.txt" 2>&1; then
      echo "::error::Romper '$1': la prueba sigue en verde con la puerta rota."; mal=1
    elif ! grep -qF -- "::error::Caso '$3'" "$tmp/roto.txt"; then
      cat "$tmp/roto.txt"; echo "::error::Romper '$1': la prueba falla, pero no por el caso '$3'."; mal=1
    else
      echo "Romper '$1': la prueba lo caza (caso '$3')."
    fi
  }
  romper 'sin filtro de .github' 's#any(test("^\\\\.github/#any(test("^\\\\.nada/#' 'toca la puerta'
  romper 'sin la mayor' 's#if \[ "${despues%%.\*}" != "${antes%%.\*}" \]; then#if false; then#' 'sube la mayor'
  romper 'sin lista blanca de MSBuild' 's#if \[ -n "$fueradelista" \]; then#if false; then#' 'versión con condición'
  romper 'MSBuild sin patch null' 's#.status != "modified" or .patch == null#.status == "nada"#' 'props sin diff'
  romper 'sin lista completa' 's#|| no "no se pudo leer la lista completa#|| echo "no se pudo leer la lista completa#' 'lista de ficheros incompleta'
fi

exit "$mal"
