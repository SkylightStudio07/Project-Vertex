Add-Type -AssemblyName System.Drawing
$sourceRoot = 'C:\Users\SKYLIGHT\.codex\generated_images\01a0fc46-a6a9-77f0-8393-820b3b34e74e'
$selected = [ordered]@{
    CompanionSkill_Order_Mockup = 'exec-e5d98c96-65f2-4e44-b5f3-5bfe0c43fd4b.png'
    CompanionSkill_CutIn_Mockup = 'exec-1264c2a5-8e20-45d6-a5c2-eb3100a79d9f.png'
    CompanionSkill_CutIn_Enhanced_Mockup = 'exec-272de98b-ac4c-4a9f-b339-b173c8713903.png'
}
$nativeDir = Join-Path $PSScriptRoot 'Generated_Originals'
[System.IO.Directory]::CreateDirectory($nativeDir) | Out-Null
$records = @()
foreach ($entry in $selected.GetEnumerator()) {
    $filename = $entry.Key + '.png'
    $native = Join-Path $nativeDir $filename
    if (-not (Test-Path -LiteralPath $native)) { Copy-Item -LiteralPath (Join-Path $sourceRoot $entry.Value) -Destination $native }
    $sourceImage = [System.Drawing.Image]::FromFile($native)
    $outputImage = [System.Drawing.Bitmap]::new(1920,1080)
    $graphics = [System.Drawing.Graphics]::FromImage($outputImage)
    try {
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.DrawImage($sourceImage,[System.Drawing.Rectangle]::new(0,0,1920,1080))
        $outputImage.Save((Join-Path $PSScriptRoot $filename),[System.Drawing.Imaging.ImageFormat]::Png)
        $records += [ordered]@{file=$filename;native_size=@($sourceImage.Width,$sourceImage.Height);delivery_size=@(1920,1080);method='Built-in image_gen, followed by bicubic size normalization. Not native 2x art.'}
    } finally { $graphics.Dispose(); $outputImage.Dispose(); $sourceImage.Dispose() }
}
[ordered]@{artifacts=$records;status='Review mockups; slicing deferred until approval';notes=@('User-approved corner triangle composition overrides prior cut-in layout; see USER_DIRECTION_OVERRIDE.md.','Generated background and item are reference-based renderings, not guaranteed pixel-identical composites.','Exact layout coordinates, uniform dimmer opacity and flat text surfaces must be enforced in future slicing/implementation.','Native images retained under Generated_Originals; all prompts retained in GENERATION_PROMPTS.json.')} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'GENERATION_MANIFEST.json') -Encoding utf8
$records | ConvertTo-Json -Depth 4

