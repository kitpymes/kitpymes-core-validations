#!/usr/bin/env bash
# La puerta de fusión no puede ver código de la rama, ni el código de la rama ver el token (Sebastián, 27/11):
# 1. ningún workflow salvo auto-merge.yml nombra secretos, de ninguna forma (secrets.X, secrets[...],
#    toJSON(secrets), secrets: inherit): GitHub sólo borra de la memoria del runner los que el workflow no nombra;
# 2. auto-merge.yml sólo nombra AUTOMERGE_TOKEN, y sólo en un job con "environment: automerge" (el entorno
#    restringido a master donde vive el secreto);
# 3. auto-merge.yml no usa acciones (ningún uses:) ni descarga o ejecuta código: ni git, ni curl, ni
#    intérpretes, ni gh run / gh pr checkout / gh repo / gh workflow / gh release;
# 4. siguen en auto-merge.yml las líneas que hacen de puerta (ficheros sensibles, lista completa, versión
#    mayor, SHA, rebase), y ci.yml tiene bloque permissions.
# ci.yml sí pide escritura (contents e id-token) en el job que publica, que sólo corre en master: por eso aquí
# no se comprueba, a diferencia del frontend. Lo que impide que una rama lo cambie y se fusione sola es que
# .github/** no se fusiona solo.
# Es una lista negra: protege contra descuidos, no es una garantía.
# Las líneas de comentario no cuentan. Uso: check-puerta-fusion.sh <carpeta de workflows>. Sale con 1 si algo falla.
set -uo pipefail
dir="${1:?carpeta de workflows}"
am="$dir/auto-merge.yml"; ci="$dir/ci.yml"
mal=0
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
lineas="$tmp/lineas"
falla() { echo "::error::Puerta de fusión: $1"; mal=1; }
# Copia sin las líneas de comentario. Se busca en el fichero y no en una tubería: con pipefail,
# "grep -q" cierra la tubería al encontrar y la tubería sale en error aunque haya coincidencia.
codigo() { local c="$tmp/$(basename "$1").codigo"; grep -vE '^[[:space:]]*#' "$1" > "$c"; echo "$c"; }

[ -f "$am" ] && [ -f "$ci" ] || { echo "::error::Faltan auto-merge.yml o ci.yml en $dir."; exit 2; }

for f in "$dir"/*.yml "$dir"/*.yaml; do
  [ -f "$f" ] || continue
  [ "$f" = "$am" ] && continue
  grep -qiw 'secrets' "$(codigo "$f")" && falla "$(basename "$f") nombra secretos; sólo puede hacerlo auto-merge.yml."
done

amc=$(codigo "$am"); cic=$(codigo "$ci")
todas=$(grep -oiw 'secrets' "$amc" | wc -l)
token=$(grep -o 'secrets\.AUTOMERGE_TOKEN\b' "$amc" | wc -l)
[ "$todas" -ge 1 ] && [ "$todas" = "$token" ] \
  || falla "auto-merge.yml sólo puede nombrar AUTOMERGE_TOKEN, como secrets.AUTOMERGE_TOKEN ($todas menciones de secrets, $token del token)."

# Cada job que nombra el token tiene "environment: automerge", y ningún otro job declara entorno.
awk '
  /^jobs:/ { enjobs = 1; next }
  enjobs && /^  [A-Za-z0-9_-]+:[[:space:]]*$/ { job = $1; sub(":", "", job); next }
  enjobs && /^    environment:[[:space:]]*automerge[[:space:]]*$/ { conentorno[job] = 1; next }
  enjobs && /environment:/ { print "el job " job " declara otro entorno o con otra forma: " $0 }
  enjobs && /secrets\.AUTOMERGE_TOKEN/ { contoken[job] = 1 }
  END {
    for (j in contoken) if (!(j in conentorno)) print "el job " j " usa AUTOMERGE_TOKEN sin environment: automerge"
    for (j in conentorno) if (!(j in contoken)) print "el job " j " tiene environment: automerge y no usa el token"
  }' "$amc" > "$lineas"
[ -s "$lineas" ] && { sed 's/^/  /' "$lineas"; falla "auto-merge.yml: el token sólo en el job con environment: automerge (líneas de arriba)."; }

grep -nwE 'uses|git|curl|wget|eval|exec|npm|npx|node|yarn|pnpm|bash|sh|zsh|python|python3|perl|ruby|make|source|docker|dotnet' "$amc" \
  | sed 's/^/  /' > "$lineas"
grep -nE '(^|[[:space:]])\./|(^|[[:space:];&|])\.[[:space:]]+[^[:space:]]|gh[[:space:]]+(run|repo|workflow|release|extension|gist)\b|gh[[:space:]]+pr[[:space:]]+(checkout|diff)\b|/contents/|/tarball|/zipball|/artifacts|/git/|graphql|GITHUB_ENV|GITHUB_PATH|BASH_ENV|\$SHELL|system\(' "$amc" \
  | sed 's/^/  /' >> "$lineas"
[ -s "$lineas" ] && { cat "$lineas"; falla "auto-merge.yml usa acciones o descarga o ejecuta código (líneas de arriba)."; }

# Nada nombra secretos fuera de los jobs (un env: global los daría a todos), y nada se salta un fallo.
awk '/^jobs:/ { exit } /secrets/ { print NR": "$0 }' "$amc" > "$lineas"
[ -s "$lineas" ] && { sed 's/^/  /' "$lineas"; falla "auto-merge.yml nombra el secreto fuera de los jobs (líneas de arriba)."; }
grep -q 'continue-on-error' "$amc" && falla "auto-merge.yml usa continue-on-error: un paso en rojo saldría en verde."

grep -qE '^permissions:' "$cic" || falla "ci.yml no tiene bloque permissions: (cogería los permisos por defecto del repo)."

# Las líneas de auto-merge.yml que hacen de puerta tienen que seguir ahí.
for linea in \
  'merge_method=rebase -f sha="$SHA_CI"' \
  '.changed_files' \
  'any(test("^\\.github/|(^|/)nuget\\.config$"; "i"))' \
  'no "el PR #$numero toca .github/** o nuget.config' \
  'no "no se pudo leer la lista completa de ficheros' \
  'no "el PR #$numero cambia la versión mayor' \
  'no "el PR #$numero cambia MSBuild más allá de la versión' \
  '.status != "modified" or .patch == null' \
  'repos/$REPO/pulls/$numero/files' \
  'props=docs/props/Pack.Build.props'; do
  grep -qF -- "$linea" "$amc" || falla "auto-merge.yml ya no tiene la línea de la puerta: $linea"
done

[ "$mal" = 0 ] && echo "Puerta de fusión: separada del código de la rama, con el token en el entorno automerge."
exit "$mal"
