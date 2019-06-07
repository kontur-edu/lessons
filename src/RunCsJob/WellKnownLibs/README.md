Директория для доступных при сборке решений библиотек.
Важно их включать в проект с типом AdditionalFiles, 
иначе MSBUILD будет использовать эти сборки для разрешения ссылок при сборке самого проекта RunCsJob.

Формат директории

```
packages
  <lib name>
	lib.dll
```

## Список библиотек

1. NUnit — версия 3.11.0, собрана для .NET Framework 4.7 из ветки `sandbox-compatible` репозитория https://github.com/andgein/nunit

2. FluentAssertion — версия 5.2.0, собрана для .NET Framework 4.7 из ветки `nunit-only` репозитория https://github.com/andgein/fluentassertions.