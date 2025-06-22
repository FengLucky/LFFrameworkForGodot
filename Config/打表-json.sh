cd "$(dirname "${BASH_SOURCE[0]}")"
(dotnet Luban/Luban.dll \
-t client \
--conf luban.conf \
--customTemplateDir Templates \
-x outputCodeDir=../Data/GenCode \
-x pathValidator.rootDir=../ \
-c cs-dotnet-json \
-d json \
-x outputDataDir=../Data/Json \
-d const-cs \
-x const-cs.outputDataDir=../Data/GenConst \
--validationFailAsError) || {
  pause
  exit 1
}
pause