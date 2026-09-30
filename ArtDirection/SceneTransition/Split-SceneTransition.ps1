Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $out | Out-Null
$source=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'SceneTransition_ComponentStoryboard_v2.png'))
$entries=[System.Collections.Generic.List[object]]::new()
function Slice($name,$x,$y,$w,$h,$ow,$oh,$mode){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($b);$g.DrawImage($source,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($x,$y,$w,$h),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose()
 if($mode -eq 'frame' -or $mode -eq 'ink'){
  for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){
   $c=$b.GetPixel($ix,$iy)
   $corner=($ix -lt 52 -or $ix -gt ($w-53)) -and ($iy -lt 27 -or $iy -gt ($h-28))
   $v=[Math]::Max($c.R,[Math]::Max($c.G,$c.B))
   $a=if($mode -eq 'frame' -and !$corner){0}else{[int](255*[Math]::Max(0.0,[Math]::Min(1.0,($v-55.0)/145.0)))}
   if($a -lt 8){$b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb(0,0,0,0))}else{$b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb($a,$c.R,$c.G,$c.B))}
  }}
 }
 if($mode -eq 'upper' -or $mode -eq 'lower'){
  $patch=$b.Clone([System.Drawing.Rectangle]::new(25,73,155,14),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $pg=[System.Drawing.Graphics]::FromImage($b);$pg.DrawImageUnscaled($patch,25,99);$pg.Dispose();$patch.Dispose()
 }
 if($mode -eq 'lower'){$b.RotateFlip([System.Drawing.RotateFlipType]::RotateNoneFlipY)}
 $result=[System.Drawing.Bitmap]::new($ow,$oh,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($result);$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.DrawImage($b,0,0,$ow,$oh);$g.Dispose();$b.Dispose()
 if($mode -eq 'upper' -or $mode -eq 'lower'){
  for($iy=0;$iy -lt $oh;$iy++){for($ix=0;$ix -lt $ow;$ix++){
   $cut=if($mode -eq 'upper'){$ix -lt [int](70*(1.0-$iy/($oh-1.0)))}else{$ix -ge ($ow-[int](70*$iy/($oh-1.0)))}
   $c=$result.GetPixel($ix,$iy);if($cut){$result.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb(0,0,0,0))}else{$result.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B))}
  }}
 }
 if($mode -eq 'chip'){
  for($iy=0;$iy -lt $oh;$iy++){for($ix=0;$ix -lt $ow;$ix++){
   $cut=($ix -lt 12 -and $iy -lt (12-$ix)) -or ($ix -gt ($ow-14) -and $iy -gt ($oh-14+($ow-$ix)))
   if($cut){$result.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb(0,0,0,0))}
  }}
 }
 $result.Save((Join-Path $out ($name+'.png')));$result.Dispose()
 $entries.Add([pscustomobject]@{name=$name;file=$name+'.png';sourceRect=@($x,$y,$w,$h);size=@($ow,$oh);mode=$mode})
}
Slice 'Curtain_Upper' 56 148 741 125 2320 548 'upper'
Slice 'Curtain_Lower' 56 339 741 125 2320 548 'lower'
Slice 'Seam_Base' 839 157 552 13 2320 24 'opaque'
Slice 'Seam_Fill' 839 236 355 7 2320 6 'opaque'
Slice 'Seam_Head' 1267 236 52 36 48 24 'ink'
Slice 'TitleFrame' 851 337 375 103 1200 300 'frame'
Slice 'Emblem' 1260 337 113 105 160 160 'ink'
Slice 'LoadingChip' 1071 499 226 32 280 44 'chip'
# Full-size assemblies from exported assets, plus labeled examples and exact source crops.
function DrawAsset($g,$name,$x,$y,$w=0,$h=0){
 $b=[System.Drawing.Bitmap]::new((Join-Path $out ($name+'.png')))
 if($w -gt 0){$g.DrawImage($b,$x,$y,$w,$h)}else{$g.DrawImageUnscaled($b,$x,$y)}
 $b.Dispose()
}
foreach($state in @('Closed','Loading','Opening','Return')){
 $canvas=[System.Drawing.Bitmap]::new(1920,1080,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($canvas)
 $g.Clear([System.Drawing.Color]::FromArgb(235,237,239))
 $shift=if($state -eq 'Opening'){1000}else{0}
 DrawAsset $g 'Curtain_Upper' (-200-$shift) 0
 DrawAsset $g 'Curtain_Lower' (-200+$shift) 532
 if($state -ne 'Opening'){
  DrawAsset $g 'Seam_Base' -200 528
  if($state -eq 'Loading'){DrawAsset $g 'Seam_Fill' -200 537 1392 6;DrawAsset $g 'Seam_Head' 1168 528}
  DrawAsset $g 'TitleFrame' 360 390
  DrawAsset $g 'Emblem' 480 370
  DrawAsset $g 'LoadingChip' 1600 1000
 }
 $g.Dispose();$canvas.Save((Join-Path $out ('Assembly_'+$state+'_Textless.png')))
 if($state -ne 'Opening'){
  $g=[System.Drawing.Graphics]::FromImage($canvas);$g.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
  $small=[System.Drawing.Font]::new('Pretendard',22,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
  $big=[System.Drawing.Font]::new('Pretendard',88,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
  $cyan=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(13,184,242));$white=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::White);$gray=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,183,188))
  $caption=if($state -eq 'Return'){'RETURN // BASE'}else{'OPERATION // DEPLOY'}
  $title=if($state -eq 'Return'){'귀환'}else{'출정'}
  $hint=if($state -eq 'Return'){'기지로 복귀합니다'}else{'작전 지역으로 이동합니다'}
  $g.DrawString($caption,$small,$cyan,720,395);$g.DrawString($title,$big,$white,755,432);$g.DrawString($hint,$small,$gray,740,580);$g.DrawString('NOW LOADING',$small,$white,1660,1007)
  $g.Dispose();$small.Dispose();$big.Dispose();$cyan.Dispose();$white.Dispose();$gray.Dispose()
 }
 $canvas.Save((Join-Path $out ('Example_'+$state+'.png')));$canvas.Dispose()
}
foreach($r in @(@('Reference_Closed',56,601,652,195),@('Reference_Loading',742,601,652,195),@('Reference_Opening',56,827,652,195),@('Reference_Return',742,827,652,195))){
 Slice $r[0] $r[1] $r[2] $r[3] $r[4] 1920 1080 'reference'
}
[ordered]@{canvas=@(1920,1080);origin='top-left';source='SceneTransition_ComponentStoryboard_v2.png';sourceCanvas=@(1448,1086);assets=$entries;placements=@{Curtain_Upper=@(-200,0);Curtain_Lower=@(-200,532);Seam_Base=@(-200,528);Seam_Fill=@(-200,537);TitleFrame=@(360,390);Emblem=@(480,370);LoadingChip=@(1600,1000)};loadingFill=.6;openingOffsets=@{upperX=-1000;lowerX=1000};opaqueCurtainInterior=$true;runtimeLabels=@{deploy=@('OPERATION // DEPLOY','출정','작전 지역으로 이동합니다');return=@('RETURN // BASE','귀환','기지로 복귀합니다')}} | ConvertTo-Json -Depth 9 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$source.Dispose()
Write-Output ('Exported '+$entries.Count+' slices and 8 full-size assemblies')
