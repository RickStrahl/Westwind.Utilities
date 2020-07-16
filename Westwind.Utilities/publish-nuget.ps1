if (test-path ./nupkg) {
    remove-item ./nupkg -Force -Recurse
}   

dotnet build -c Release

# $filename = 'LiveReloadServer.0.2.4.nupkg'
$filename = gci "./nupkg/*.nupkg" | sort LastWriteTime | select -last 1 | select -ExpandProperty "Name"
Write-host $filename
$len = $filename.length

if ($len -gt 0) {
    Write-Host "signing... $filename"
    nuget sign  ".\nupkg\$filename"   -CertificateSubject "West Wind Technologies" -timestamper " http://timestamp.digicert.com"
    
    
    $filename = $filename.Replace(".nupkg",".snupkg")
    Write-Host "signing... $filename"
    nuget sign  ".\nupkg\$filename"   -CertificateSubject "West Wind Technologies" -timestamper " http://timestamp.digicert.com"
    
    cd ./nupkg
    # nuget push  "$filename" -source nuget.org
    cd ..
}