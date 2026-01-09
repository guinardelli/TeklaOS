# Script para criar pacote TSEP
param(
    [string]$ManifestPath = (Join-Path $PSScriptRoot "..\tsep\RelatorioModelo\Manifest.xml"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\tsep\RelatorioModelo\output")
)

$ErrorActionPreference = "Stop"

# Verificar se o arquivo de manifesto existe
if (!(Test-Path $ManifestPath)) {
    throw "Arquivo de manifesto nao encontrado: $ManifestPath"
}

# Verificar se o diretório de saída existe
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Caminho para o Tekla Extension Package Builder
$teklaBinPath = "C:\TeklaStructures\2023.0\bin"
$builderPath = Join-Path $teklaBinPath "TeklaExtensionPackage.Builder.exe"

# Verificar se o Builder existe
if (!(Test-Path $builderPath)) {
    throw "TeklaExtensionPackage.Builder.exe nao encontrado em: $builderPath"
}

# Executar o Builder
Write-Host "Criando pacote TSEP..."
Write-Host "Manifest: $ManifestPath"
Write-Host "Output: $OutputPath"

try {
    # Usar caminho absoluto para evitar problemas
    $manifestAbsolutePath = Resolve-Path $ManifestPath
    $outputAbsolutePath = Resolve-Path $OutputPath
    
    # Executar o comando com os caminhos absolutos
    & $builderPath /definition:"$manifestAbsolutePath" /output:"$outputAbsolutePath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCESSO] Pacote TSEP criado com sucesso."
    } else {
        Write-Warning "[AVISO] O comando retornou codigo de erro: $LASTEXITCODE"
    }
} catch {
    Write-Error "Erro ao executar o TeklaExtensionPackage.Builder.exe: $_"
}