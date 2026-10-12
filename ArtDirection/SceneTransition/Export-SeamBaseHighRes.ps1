Add-Type -AssemblyName System.Drawing
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Seam_Base_HighRes_Model.png'))
$rows=@()
for($y=0;$y -lt $src.Height;$y++){
 $count=0
 for($x=0;$x -lt $src.Width;$x+=16){$c=$src.GetPixel($x,$y);if($c.B-$c.R -gt 12 -or $c.R -gt 140){$count++}}
 if($count -gt ($src.Width/16*.6)){$rows+=$y}
}
if($rows.Count -eq 0){throw 'Seam band not found'}
$top=($rows | Measure-Object -Minimum).Minimum
$bottom=($rows | Measure-Object -Maximum).Maximum
$result=[System.Drawing.Bitmap]::new(2320,24,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[System.Drawing.Graphics]::FromImage($result)
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($src,[System.Drawing.Rectangle]::new(0,0,2320,24),[System.Drawing.Rectangle]::new(0,$top,$src.Width,($bottom-$top+1)),[System.Drawing.GraphicsUnit]::Pixel)
$g.Dispose()
$target=Join-Path $PSScriptRoot 'Extracted/Seam_Base.png'
$backup=Join-Path $PSScriptRoot 'Extracted/Seam_Base_LowRes_Backup.png'
if(!(Test-Path $backup)){Copy-Item -LiteralPath $target -Destination $backup}
$result.Save($target,[System.Drawing.Imaging.ImageFormat]::Png)
$result.Save((Join-Path $PSScriptRoot 'Extracted/Seam_Base_HighRes.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$result.Dispose();$src.Dispose()
Write-Output "Saved 2320x24 seam; model band rows $top..$bottom"
