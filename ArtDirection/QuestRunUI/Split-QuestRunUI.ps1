Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'QuestRunUI_ComponentSheet_v1.png'))
$model=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'QuestRunUI_Textless_Model.png'))
if($model.Size -ne $src.Size){throw 'Master size mismatch'}
$clean=$src.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$erasures=@(@(217,106,337,66),@(695,113,57,16),@(216,212,279,66),@(695,218,57,15),@(217,320,390,67),@(216,436,402,66),@(694,442,61,16),@(911,110,277,34),@(941,177,127,25),@(1205,177,122,25),@(939,296,130,28),@(1204,296,127,28),@(1447,297,173,24),@(906,405,292,36),@(936,461,426,66),@(1298,512,142,26),@(541,693,241,28),@(654,327,80,44))
$g=[System.Drawing.Graphics]::FromImage($clean)
foreach($e in $erasures){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($model,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}
$g.DrawImage($model,[System.Drawing.Rectangle]::new(906,405,292,36),[System.Drawing.Rectangle]::new(1140,405,292,36),[System.Drawing.GraphicsUnit]::Pixel)
$g.Dispose();$model.Dispose()
$entries=[System.Collections.Generic.List[object]]::new()
function Export($name,$x,$y,$w,$h,$ow,$oh,$polygon=$null,$mode='paper',$reference=$false){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b)
 if($polygon){$path=[System.Drawing.Drawing2D.GraphicsPath]::new();$pts=[System.Collections.Generic.List[System.Drawing.PointF]]::new();for($i=0;$i -lt $polygon.Count;$i+=2){$pts.Add([System.Drawing.PointF]::new(($polygon[$i]-$x),($polygon[$i+1]-$y)))};$path.AddPolygon($pts.ToArray());$g.SetClip($path)}
 $im=if($reference){$src}else{$clean};$g.DrawImage($im,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($x,$y,$w,$h),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose()
 if($mode -eq 'slot'){
  # Only cyan/white flag pixels and outer frame survive; checkerboard center becomes genuine alpha.
  for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){
   $c=$b.GetPixel($ix,$iy);$badge=$ix -lt 50 -and $iy -gt 92
   $inner=$ix -gt 14 -and $ix -lt ($w-14) -and $iy -gt 14 -and $iy -lt ($h-14)
   if($inner -and !$badge){$b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb(0,0,0,0))}
  }}
 }
 if($mode -eq 'glow'){
  for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){
   $c=$b.GetPixel($ix,$iy)
   $core=$ix -ge 12 -and $ix -lt ($w-12) -and $iy -ge 12 -and $iy -lt ($h-12)
   if(!$core){$a=[int](255*[Math]::Max(0.0,[Math]::Min(1.0,([double]$c.B-$c.R)/180.0)));$b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb($a,13,184,242))}
  }}
 }
 $result=[System.Drawing.Bitmap]::new($ow,$oh,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($result);$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.DrawImage($b,0,0,$ow,$oh);$g.Dispose();$b.Dispose()
 $result.Save((Join-Path $out ($name+'.png')));$result.Dispose()
 if($polygon){$path.Dispose()}
 $entries.Add([pscustomobject]@{name=$name;file=$name+'.png';sourceRect=@($x,$y,$w,$h);size=@($ow,$oh);kind=$mode;reference=$reference})
}
$toastSpecs=@(@('Acquired',76,92,704,94),@('Progress',76,198,704,94),@('Completed',75,304,707,98),@('Deferred',76,421,704,94))
foreach($t in $toastSpecs){
 $poly=@(($t[1]+10),$t[2],($t[1]+$t[3]),$t[2],($t[1]+$t[3]),($t[2]+$t[4]-19),($t[1]+$t[3]-20),($t[2]+$t[4]),$t[1],($t[2]+$t[4]),$t[1],($t[2]+10))
 Export ('Toast_'+$t[0]+'_Normal') $t[1] $t[2] $t[3] $t[4] 420 76 $poly
 Export ('Example_Toast_'+$t[0]) $t[1] $t[2] $t[3] $t[4] 420 76 $poly 'paper' $true
}
Export 'Summary_Header' 880 97 499 56 310 24
Export 'Chip_Recovery_Normal' 880 161 239 58 150 30
Export 'Chip_Delivery_Normal' 1140 161 240 58 150 30
Export 'Chip_Normal' 880 280 214 54 150 30
Export 'Chip_Normal_Hover' 1140 280 214 54 150 30
Export 'Summary_Empty' 1397 281 229 53 310 30
Export 'Tooltip_Normal' 880 390 573 157 300 110 @(890,390,1453,390,1453,522,1429,547,880,547,880,401)
Export 'Example_Tooltip' 880 390 573 157 300 110 @(890,390,1453,390,1453,522,1429,547,880,547,880,401) 'paper' $true
Export 'Example_Summary_Header' 880 97 499 56 310 24 $null 'reference' $true
Export 'Example_Summary_RecoveryChip' 880 161 239 58 150 30 $null 'reference' $true
Export 'Example_Summary_DeliveryChip' 1140 161 240 58 150 30 $null 'reference' $true
Export 'MapTooltip_Normal' 507 680 283 74 220 40 @(517,680,790,680,790,711,767,734,647,734,639,754,631,734,507,734,507,691)
Export 'Example_MapTooltip' 507 680 283 74 220 40 @(517,680,790,680,790,711,767,734,647,734,639,754,631,734,507,734,507,691) 'paper' $true
$names=@('Recovery','Delivery','Elimination','Rescue')
for($i=0;$i -lt 4;$i++){
 $x=@(72,165,258,351)[$i]
 Export ('MapTag_'+$names[$i]+'_Normal') $x 691 67 67 28 28 @(($x+15),691,($x+67),691,($x+67),742,($x+51),758,$x,758,$x,707)
 # Glow uses larger canvas: visual flag 28px plus 5px margins.
 Export ('MapTag_'+$names[$i]+'_Highlight') ($x-14) 796 95 98 40 40 $null 'glow'
}
Export 'ItemSlot_Normal' 905 701 148 145 70 70 @(917,701,1053,701,1053,817,1024,846,905,846,905,713) 'slot'
Export 'ItemSlot_Normal_Hover' 1135 701 149 145 70 70 @(1147,701,1284,701,1284,817,1255,846,1135,846,1135,713) 'slot'
Export 'Example_MapNode' 608 767 122 121 110 80 $null 'reference' $true
Export 'Example_ItemSlot' 1417 700 148 145 70 70 $null 'reference' $true
# Restore typed icons from the existing questboard kit as requested in the brief.
foreach($name in $names){
 $p=Join-Path $PSScriptRoot ('../QuestBoardMockup/Extracted/Icon_'+$name+'.png')
 if(Test-Path $p){Copy-Item -LiteralPath $p -Destination (Join-Path $out ('Icon_'+$name+'.png'))}
}
$clean.Save((Join-Path $out 'Textless_Master.png'))
$summary=[System.Drawing.Bitmap]::new(310,56,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$sg=[System.Drawing.Graphics]::FromImage($summary)
foreach($part in @(@('Example_Summary_Header',0,0),@('Example_Summary_RecoveryChip',0,26),@('Example_Summary_DeliveryChip',160,26))){$b=[System.Drawing.Bitmap]::new((Join-Path $out ($part[0]+'.png')));$sg.DrawImageUnscaled($b,$part[1],$part[2]);$b.Dispose()}
$sg.Dispose();$summary.Save((Join-Path $out 'Example_Summary.png'));$summary.Dispose()
[ordered]@{source='QuestRunUI_ComponentSheet_v1.png';sourceCanvas=@(1672,941);assets=$entries;placements=@{toast=@{canvas=@(1920,1080);position=@(1470,130);size=@(420,76);stackGap=8;maxCount=3};summary=@{canvas=@(1672,941);position=@(1100,26);size=@(310,56);chipPositions=@(@(0,26),@(160,26))};mapTag=@{normalSize=@(28,28);highlightSize=@(40,40);highlightOffset=@(-6,-6);anchor='node top-right'};itemSlot=@{size=@(70,70);anchor='slot center';transparentCenter=$true}}} | ConvertTo-Json -Depth 9 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$src.Dispose();$clean.Dispose()
Write-Output ('Exported '+$entries.Count+' assets plus shared type icons')
