cd "$(dirname "${BASH_SOURCE[0]}")"
(dotnet Luban/Luban.dll \
-t client \
--conf luban.conf \
--customTemplateDir Templates \
-x outputCodeDir=../Data/GenCode \
-x pathValidator.rootDir=../ \
-c cs-bin \
-d bin \
-x outputDataDir=../Data/Bin \
-d const-cs \
-x const-cs.outputDataDir=../Data/GenConst \
-x tableImporter.name=lf \
-x tableImporter.tableMeta=TableMeta.ini \
--validationFailAsError) || {
  pause
  exit 1
}
pause