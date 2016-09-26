properties {
    $base_dir       = (Get-Item (Resolve-Path .)).Parent.FullName
    $bin_dir        = "$base_dir\bin"
    $sln_path       = "$base_dir\src\log4net.ElasticSearch.sln"
    $config         = "Debug"
	$platform       = "Any CPU"
    $tests_path     = "$base_dir\src\log4net.ElasticSearch.Tests\bin\$config\log4net.ElasticSearch.Tests.dll"
    $xunit_path     = "$base_dir\src\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe"
    $dirs           = @($bin_dir)
    $artefacts      = @("$base_dir\LICENSE", "$base_dir\readme.txt")
    $nuget_path     = "$base_dir\tools\nuget\NuGet.exe"
    $nuspec_path    = "$base_dir\scripts\log4net.ElasticSearch.nuspec"
	$source         = "https://mediamarkt.myget.org/F/default/api/v2/package"
	$apiKey         = "74faaa07-***"
}

task default        -depends Clean, Compile, Test
task Package        -depends default, CopyArtefactsToBinDirectory, CreateNugetPackage
task Publish        -depends Package, SetApiKey, Push

task Clean {
    $dirs | % { Recreate-Directory $_ }
}

task Compile {
    exec {
        msbuild $sln_path /p:Configuration=$config /p:Platform=$platform /t:Rebuild /v:Quiet /nologo
    }
}

task Test {
    exec {
        & $xunit_path $tests_path
    }
}

task CopyArtefactsToBinDirectory {
    $artefacts | % { CopyTo-BinDirectory $_ }
}

task CreateNugetPackage {
    exec {
        & $nuget_path pack $nuspec_path -Basepath $bin_dir -OutputDirectory $bin_dir
    }
}

task ? -Description "Helper to display task info" {
    Write-Documentation
}

task SetApiKey {
	exec {
        & $nuget_path setApiKey "$apiKey" '-Source' "$source"
    }
}

task Push {
	exec {
		& $nuget_path push "$bin_dir\*.nupkg" '-Source' "$source"
    }
}


function Recreate-Directory($directory) {
    if (Test-Path $directory) {
        Write-Host -NoNewline  "`tDeleting $directory"
        Remove-Item $directory -Recurse -Force | out-null
        Write-Host "...Done"
    }

    Write-Host -NoNewline  "`tCreating $directory"
    New-Item $directory -Type Directory | out-null
    Write-Host "...Done"
}

function CopyTo-BinDirectory($artefact) {
    Write-Host -NoNewline  "`tCopying $artefact to $bin_dir"
    Copy-Item $artefact $bin_dir
    Write-Host "...Done"
}
