Add-Type -AssemblyName System.Drawing
$jobs=@(
 @('QuestBoardMockup/Extracted/Card_Accepted.png','QuestBoardMockup/Card_Accepted_NoArt_Model.png','QuestBoardMockup/Extracted/Card_Accepted_NoArt.png'),
 @('ArmoryMockup/CardCatalogExtracted/Card_Attack.png','ArmoryMockup/Card_Attack_NoArt_Model.png','ArmoryMockup/CardCatalogExtracted/Card_Attack_NoArt.png')
)
$manifest=@()
foreach($j in $jobs){
 $source=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot $j[0]))
 $model=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot $j[1]))
 $result=[System.Drawing.Bitmap]::new($source.Width,$source.Height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($result)
 $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $g.DrawImage($model,0,0,$source.Width,$source.Height)
 $g.Dispose()
 # Preserve exact original outer border pixels; interiors come from model edits.
 for($y=0;$y -lt $source.Height;$y++){for($x=0;$x -lt $source.Width;$x++){
  if($x -lt 4 -or $y -lt 4 -or $x -ge ($source.Width-4) -or $y -ge ($source.Height-4)){$result.SetPixel($x,$y,$source.GetPixel($x,$y))}
 }}
 $result.Save((Join-Path $PSScriptRoot $j[2]),[System.Drawing.Imaging.ImageFormat]::Png)
 $manifest+= [pscustomobject]@{source=$j[0];emptyVariant=$j[2];width=$source.Width;height=$source.Height;interior='model-restored blank paper';originalPreserved=$true}
 $source.Dispose();$model.Dispose();$result.Dispose()
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'EmptyCardVariants.json') -Encoding utf8
$manifest | Format-Table
