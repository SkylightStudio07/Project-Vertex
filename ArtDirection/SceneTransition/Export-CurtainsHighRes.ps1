Add-Type -AssemblyName System.Drawing
foreach($name in @('Upper','Lower')){
 $src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot ('Curtain_'+$name+'_HighRes_Model.png')))
 $rows=@()
 for($y=0;$y -lt $src.Height;$y++){
  $dark=0
  foreach($fraction in @(.3,.4,.5,.6,.7)){$c=$src.GetPixel([int]($src.Width*$fraction),$y);if([Math]::Max($c.R,[Math]::Max($c.G,$c.B)) -lt 65){$dark++}}
  if($dark -eq 5){$rows+=$y}
 }
 if($rows.Count -lt 200){throw 'Curtain surface bounds not found'}
 $top=($rows | Measure-Object -Minimum).Minimum;$bottom=($rows | Measure-Object -Maximum).Maximum
 $result=[System.Drawing.Bitmap]::new(2320,548,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($result);$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $g.DrawImage($src,[System.Drawing.Rectangle]::new(0,0,2320,548),[System.Drawing.Rectangle]::new(0,$top,$src.Width,($bottom-$top+1)),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose()
 if($name -eq 'Lower'){
  $patch=$result.Clone([System.Drawing.Rectangle]::new(450,0,250,190),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $pg=[System.Drawing.Graphics]::FromImage($result);$pg.DrawImageUnscaled($patch,0,0);$pg.Dispose();$patch.Dispose()
 }
 for($y=0;$y -lt 548;$y++){for($x=0;$x -lt 2320;$x++){
  $c=$result.GetPixel($x,$y)
  $corner=if($name -eq 'Upper'){$x+$y -lt 70}else{(2319-$x)+(547-$y) -lt 70}
  if($corner -or ([Math]::Min($c.R,[Math]::Min($c.G,$c.B)) -gt 230 -and ($x -lt 95 -or $x -gt 2224))){$result.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(0,0,0,0))}else{$result.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B))}
 }}
 $target=Join-Path $PSScriptRoot ('Extracted/Curtain_'+$name+'.png')
 $backup=Join-Path $PSScriptRoot ('Extracted/Curtain_'+$name+'_LowRes_Backup.png')
 if(!(Test-Path $backup)){Copy-Item -LiteralPath $target -Destination $backup}
 $result.Save($target,[System.Drawing.Imaging.ImageFormat]::Png)
 $result.Save((Join-Path $PSScriptRoot ('Extracted/Curtain_'+$name+'_HighRes.png')),[System.Drawing.Imaging.ImageFormat]::Png)
 $src.Dispose();$result.Dispose()
 Write-Output ($name+': 2320x548; model crop rows '+$top+'..'+$bottom)
}
