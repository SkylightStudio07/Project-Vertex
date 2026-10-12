Add-Type -AssemblyName System.Drawing
$destination = $PSScriptRoot
$sourceRoot = 'C:\Users\SKYLIGHT\.codex\generated_images\01a0fc46-a6a9-77f0-8393-820b3b34e74e'
$chosen = [ordered]@{
    Closed = 'exec-85c660fb-d8d9-4dee-b31c-6bb9d7a3e28e.png'
    Loading = 'exec-5942f89e-261c-4f0d-a1e6-91cfca19e64f.png'
    Moving = 'exec-81c40661-bad1-4db3-855b-7452c638dd23.png'
    Opening = 'exec-b79b6372-b13d-43cf-b0c6-bbef6e4bb54e.png'
}
$nativeDir = Join-Path $destination 'Generated_Originals'
[System.IO.Directory]::CreateDirectory($nativeDir) | Out-Null
$records = @()
foreach ($entry in $chosen.GetEnumerator()) {
    $name = 'SceneTransition_v2_' + $entry.Key + '.png'
    $native = Join-Path $nativeDir $name
    if (-not (Test-Path -LiteralPath $native)) {
        Copy-Item -LiteralPath (Join-Path $sourceRoot $entry.Value) -Destination $native
    }
    $inputImage = [System.Drawing.Image]::FromFile($native)
    $outputImage = [System.Drawing.Bitmap]::new(1920,1080)
    $graphics = [System.Drawing.Graphics]::FromImage($outputImage)
    try {
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.DrawImage($inputImage,[System.Drawing.Rectangle]::new(0,0,1920,1080))
        $outputImage.Save((Join-Path $destination $name),[System.Drawing.Imaging.ImageFormat]::Png)
        $records += [ordered]@{file=$name;native_size=@($inputImage.Width,$inputImage.Height);delivery_size=@(1920,1080);method='Built-in image_gen; bicubic delivery-size normalization only; not native 4K or 2x sprite art'}
    } finally {
        $graphics.Dispose()
        $outputImage.Dispose()
        $inputImage.Dispose()
    }
}
$report = [ordered]@{
    artifacts=$records
    status='Concept mockups for review; not sliced runtime assets'
    notes=@('Paper/ink/cyan design follows lobby and result references.','Closed: departure 0%; Loading: return about60%; Moving: training closing over lobby; Opening: battle reveal.','Generated typography, reference-background details and geometry may differ from source; exact coordinates and untouched game backdrop must be enforced during implementation.','Native output is1672x941, retained in Generated_Originals.1920x1080 deliveries are normalized copies, not native high-resolution redraws.','Future split must redraw geometry at2x/PPU200 from approved design rather than upscale these mockups.')
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $destination 'GENERATION_MANIFEST_v2.json') -Encoding utf8
$records | ConvertTo-Json -Depth 4
