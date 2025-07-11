@echo off
cd /d %~dp0
dotnet Luban\Luban.dll ^
-t client ^
--conf luban.conf ^
--customTemplateDir Templates ^
-x outputCodeDir=..\Data\GenCode ^
-x pathValidator.rootDir=..\ ^
-c cs-dotnet-json ^
-d json ^
-x outputDataDir=..\Data\Json ^
-d const-cs ^
-x const-cs.outputDataDir=..\Data\GenConst ^
-x tableImporter.name=lf ^
-x tableImporter.tableMeta=TableMeta.ini ^
--validationFailAsError
pause
if %errorlevel% neq 0 exit /b %errorlevel%