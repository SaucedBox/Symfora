# A Little Ditty
For backup and systematic editing purposes only, between your Mac and PC.
Leave .csproj as is, don't ever edit it without putting changes below.

## .csproj Changes
```xml
<!--These changes are for compatibility only.
FOR MAC ONLY:-->

<!--Syfmora.csproj, <ProertyGroup>-->
  <DotnetCommand>/usr/local/share/dotnet/x64/dotnet</DotnetCommand>
<!--TripleS.csproj, <ProertyGroup>-->
  <DotnetCommand>/usr/local/share/dotnet/x64/dotnet</DotnetCommand>
<!--TripleS.csproj, <ItemGroup> with the rest. ON WINDOWS THESE SHOULD BE .dll-->
  <None Update="fmod.dylib">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="fmodstudio.dylib">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
```
