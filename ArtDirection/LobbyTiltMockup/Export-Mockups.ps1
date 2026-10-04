Add-Type -AssemblyName System.Drawing
$sourceRoot = 'C:\Users\SKYLIGHT\.codex\generated_images\01a0fc46-a6a9-77f0-8393-820b3b34e74e'
$selected = [ordered]@{
    LobbyTilt_Mockup = 'exec-1295e4c8-537c-42fc-95c6-cf6f08d66ec9.png'
    LobbyTilt_Mockup_Hover = 'exec-f340efeb-37fc-4d5b-80b0-6c32859d7f05.png'
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
[ordered]@{artifacts=$records;status='Generated composition studies, not pixel-identical production composites';notes=@('Six floating menu cards; scene visible in gaps; separate corner framing; opaque ink thickness plus right-side scene dimming.','Hover proposes about103% training tile scale with longer shadow. Exact perspective angles are not measured in these raster mockups.','Generative edits changed some character/button/background details. Original artwork must be reused for production.','Native outputs are retained.2x frame/shadow pieces and Background_RightDim production texture are deferred to separate slicing work.');prompt_file='GENERATION_PROMPTS.json'} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'GENERATION_MANIFEST.json') -Encoding utf8
$records | ConvertTo-Json -Depth 4
