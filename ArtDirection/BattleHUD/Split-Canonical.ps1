$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'CanonicalExtracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'BattleHUD_Mockup_v3_Canonical.png'))
$skin=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Canonical_EmptyHUD_Model.png'))
$map=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'BattleHUD_Mockup_v3_Oratio_TurnBanner.png'))
if($src.Width -ne 1672 -or $src.Height -ne 941){throw 'Unexpected canonical size'}
if($skin.Size -ne $src.Size){$temp=$skin;$skin=[System.Drawing.Bitmap]::new(1672,941);$g=[System.Drawing.Graphics]::FromImage($skin);$g.DrawImage($temp,0,0,1672,941);$g.Dispose();$temp.Dispose()}
$entries=[System.Collections.Generic.List[object]]::new()
$dark=[System.Drawing.Color]::FromArgb(255,22,24,27)
$white=[System.Drawing.Color]::FromArgb(242,243,244)
$cyan=[System.Drawing.Color]::FromArgb(13,184,242)
function SaveAsset($b,$name,$rect,$kind,$extra=@{}){
 $b.Save((Join-Path $out ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
 $entry=[ordered]@{name=$name;file=$name+'.png';size=@($b.Width,$b.Height);kind=$kind;canonicalRect=$rect}
 if($rect){$entry.placement1920=@([Math]::Round($rect[0]*1920/1672),[Math]::Round($rect[1]*1080/941));$entry.displaySize1920=@([Math]::Round($rect[2]*1920/1672),[Math]::Round($rect[3]*1080/941))}
 foreach($k in $extra.Keys){$entry[$k]=$extra[$k]};$entries.Add([pscustomobject]$entry)
}
function Crop($im,$rect,$w,$h){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($b);$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $g.DrawImage($im,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($rect[0],$rect[1],$rect[2],$rect[3]),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose();return ,$b
}
function Plate($name,$rect,$w,$h,$cut=0){
 $b=Crop $skin $rect $w $h
 for($y=0;$y -lt $h;$y++){for($x=0;$x -lt $w;$x++){
  $c=$b.GetPixel($x,$y)
  $outside=($cut -gt 0) -and (($x+$y -lt $cut) -or (($w-1-$x)+($h-1-$y) -lt $cut))
  if($name -eq 'TopBar'){$outside=$x -lt (52*$y/($h-1)) -or $x -ge ($w-52*$y/($h-1))}
  if($outside){$b.SetPixel($x,$y,[System.Drawing.Color]::Transparent)}else{
   # Opaque surface: keep restrained raster texture, suppress residual generated lettering.
   $v=[int][Math]::Max(18,[Math]::Min(34,($c.R+$c.G+$c.B)/3.0))
   $b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(255,$v,($v+2),($v+5)))
  }
 }}
 SaveAsset $b $name $rect 'model-restored-canonical-panel';$b.Dispose()
}
function Variant($normal,$name,$state){
 $b=[System.Drawing.Bitmap]::new((Join-Path $out ($normal+'.png')))
 $e=$entries | Where-Object name -eq $normal
 if($state -eq 'Pressed' -or $state -eq 'Disabled'){
  for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){$c=$b.GetPixel($x,$y);if($c.A -gt 0){$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(255,32,35,40))}}}
 }
 if($state -eq 'Hover'){$g=[System.Drawing.Graphics]::FromImage($b);$p=[System.Drawing.Pen]::new($cyan,1);$g.DrawRectangle($p,1,1,($b.Width-3),($b.Height-3));$p.Dispose();$g.Dispose()}
 SaveAsset $b $name $e.canonicalRect 'state-variant' @{normal=$normal;state=$state};$b.Dispose()
}
function Ink($im,$name,$rect,$w,$h){
 $b=Crop $im $rect $w $h
 for($y=0;$y -lt $h;$y++){for($x=0;$x -lt $w;$x++){
  $c=$b.GetPixel($x,$y);$v=($c.R+$c.G+$c.B)/3.0;$a=[int](255*[Math]::Max(0.0,[Math]::Min(1.0,($v-55)/170)))
  if($name -eq 'Icon_Weapon_Pistol'){$a=[int](255*[Math]::Max(0.0,[Math]::Min(1.0,$v/70.0)))}
  $b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($a,242,243,244))
 }}
 SaveAsset $b $name $rect 'source-icon-luminance-alpha';$b.Dispose()
}
function Line($name,$w,$h,$points,$color,$width=1){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b)
 $g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias;$pen=[System.Drawing.Pen]::new($color,$width)
 for($i=0;$i -lt ($points.Count-2);$i+=2){$g.DrawLine($pen,[single]$points[$i],[single]$points[$i+1],[single]$points[$i+2],[single]$points[$i+3])}
 $pen.Dispose();$g.Dispose();if($name -eq 'Line_Sweep'){$b.SetPixel(1918,0,$cyan);$b.SetPixel(1919,1,$cyan)};SaveAsset $b $name $null 'raster-hairline-to-spec';$b.Dispose()
}
Plate 'TopBar' @(270,0,1115,64) 1300 72 27
Plate 'ItemSlot_Empty' @(1249,11,63,62) 56 56
Variant 'ItemSlot_Empty' 'ItemSlot_Hover' 'Hover'
Variant 'ItemSlot_Empty' 'ItemSlot_Quest' 'Hover'
Plate 'Button_Map_Normal' @(1482,19,73,74) 64 64
Plate 'Button_Deck_Normal' @(1574,20,74,73) 64 64
foreach($n in @('Map','Deck')){foreach($s in @('Hover','Pressed')){Variant ('Button_'+$n+'_Normal') ('Button_'+$n+'_'+$s) $s}}
Plate 'Button_EndTurn_Normal' @(1364,817,284,63) 280 70
foreach($s in @('Hover','Pressed','Disabled')){Variant 'Button_EndTurn_Normal' ('Button_EndTurn_'+$s) $s}
Plate 'Res_Panel' @(1435,614,225,192) 264 225
Variant 'Res_Panel' 'Res_Panel_EnemyTurn' 'Disabled'
Plate 'TextBand' @(1109,665,133,22) 120 40
Plate 'EnemyName_Plate' @(1109,665,133,22) 240 32
Line 'Res_Divider' 244 2 @(0,0,243,0) ([System.Drawing.Color]::FromArgb(58,62,68))
Line 'Res_Slash' 40 80 @(1,78,38,1) $white
Line 'Hairline_Slash_White' 40 80 @(1,78,38,1) $white
Line 'Hairline_Slash_Dark' 40 80 @(1,78,38,1) $dark
Line 'Hairline_Dash' 120 2 @(0,0,119,0) $white
Line 'Underline' 200 2 @(0,0,198,0,199,1,198,1) $white
Line 'Line_Sweep' 1920 2 @(0,0,1917,0) $dark
Line 'Slash_Long' 360 620 @(1,618,358,1) $dark
Line 'EndTurn_CornerSlash' 48 70 @(0,69,47,0) $white
$b=[System.Drawing.Bitmap]::new(720,128,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b);$pen=[System.Drawing.Pen]::new($white,1)
foreach($p in @(@(0,20,0,0,20,0),@(699,0,719,0,719,20),@(0,107,0,127,20,127),@(699,127,719,127,719,107))){$g.DrawLine($pen,$p[0],$p[1],$p[2],$p[3]);$g.DrawLine($pen,$p[2],$p[3],$p[4],$p[5])};$pen.Dispose();$g.Dispose();SaveAsset $b 'Bracket_Frame' $null 'raster-corner-brackets';$b.Dispose()
Ink $map 'Icon_Map' @(1493,31,51,52) 32 32
Ink $src 'Icon_Deck' @(1588,38,46,39) 32 32
Ink $src 'Icon_Energy' @(1450,680,29,47) 32 32
Ink $src 'Icon_Ammo' @(1451,768,24,34) 32 32
Ink $src 'Icon_Arrow' @(1598,838,33,21) 32 32
foreach($n in @('Arrow','Energy')){
 $b=[System.Drawing.Bitmap]::new((Join-Path $out ('Icon_'+$n+'.png')))
 for($y=0;$y -lt 32;$y++){for($x=0;$x -lt 32;$x++){$c=$b.GetPixel($x,$y);$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($c.A,90,96,105))}}
 SaveAsset $b ('Icon_'+$n+'_Disabled') $null 'disabled-icon';$b.Dispose()
}
$pistol=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Canonical_PistolIcon_Model.png'))
Ink $pistol 'Icon_Weapon_Pistol' @(0,0,$pistol.Width,$pistol.Height) 32 32;$pistol.Dispose()
# Quest corner badge is a separate tiny icon layer, never baked item art.
$quest=[System.Drawing.Bitmap]::new(16,16,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($quest);$p=[System.Drawing.Pen]::new($white,1);$g.DrawRectangle($p,3,4,10,9);$g.DrawLine($p,3,7,13,7);$p.Dispose();$g.Dispose();SaveAsset $quest 'Icon_QuestRecovery' $null 'small-box-line-symbol';$quest.Dispose()
$tmp=[System.Drawing.Bitmap]::new((Join-Path $out 'ItemSlot_Quest.png'));$q=[System.Drawing.Bitmap]::new(56,56,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($q);$g.DrawImageUnscaled($tmp,0,0);$tmp.Dispose();$qi=[System.Drawing.Bitmap]::new((Join-Path $out 'Icon_QuestRecovery.png'));$g.DrawImageUnscaled($qi,2,38);$g.Dispose();$qi.Dispose();$q.Save((Join-Path $out 'ItemSlot_Quest.png'),[System.Drawing.Imaging.ImageFormat]::Png);$q.Dispose()
$plus=[System.Drawing.Bitmap]::new(16,16,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($plus);$p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(138,144,153),1);$g.DrawLine($p,8,1,8,14);$g.DrawLine($p,1,8,14,8);$p.Dispose();$g.Dispose();SaveAsset $plus 'Icon_SlotPlus' $null 'empty-slot-symbol';$plus.Dispose()
function Health($prefix,$w,$h,$iw,$ih,$ox,$oy,$rect){
 $b=Crop $skin $rect $w $h;$g=[System.Drawing.Graphics]::FromImage($b);$brush=[System.Drawing.SolidBrush]::new($dark);$g.FillRectangle($brush,2,2,($w-4),($h-4));$brush.Dispose();$g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy;$brush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::Transparent);$g.FillRectangle($brush,$ox,$oy,$iw,$ih);$brush.Dispose();$g.Dispose();SaveAsset $b ($prefix+'_Frame') $rect 'canonical-health-frame' @{fillOffset=@($ox,$oy)};$b.Dispose()
 foreach($kind in @('Fill','Track')){
  $b=[System.Drawing.Bitmap]::new($iw,$ih,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b);$c=if($kind -eq 'Fill'){[System.Drawing.Color]::FromArgb(201,206,212)}else{[System.Drawing.Color]::FromArgb(32,35,40)};$g.Clear($c)
  if($kind -eq 'Fill'){$brush=[System.Drawing.SolidBrush]::new($cyan);$g.FillRectangle($brush,($iw-3),0,3,$ih);$brush.Dispose()};$g.Dispose();SaveAsset $b ($prefix+'_'+$kind) $null 'flat-health-surface';$b.Dispose()
 }
}
Health 'HP' 360 39 340 27 8 6 @(89,826,335,42)
Health 'EnemyHP' 240 26 227 18 5 4 @(1108,637,266,24)
function Shield($name,$w,$h,$rect){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b)
 $pts=[System.Drawing.Point[]]@([System.Drawing.Point]::new(1,1),[System.Drawing.Point]::new(($w-2),1),[System.Drawing.Point]::new(($w-2),[int]($h*.72)),[System.Drawing.Point]::new([int]($w/2),($h-2)),[System.Drawing.Point]::new(1,[int]($h*.72)))
 $path=[System.Drawing.Drawing2D.GraphicsPath]::new();$path.AddPolygon($pts);$g.SetClip($path);$g.DrawImage($skin,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($rect[0],$rect[1],$rect[2],$rect[3]),[System.Drawing.GraphicsUnit]::Pixel);$path.Dispose();$g.Dispose();SaveAsset $b $name $rect 'canonical-shield-outline-no-number';$b.Dispose()
}
Shield 'Block_Badge' 56 68 @(38,823,54,61)
Shield 'EnemyBlock_Badge' 40 48 @(1110,599,25,33)
Line 'Status_Underline' 44 2 @(0,0,43,0) $white
function DrawAsset($g,$name,$rect){$b=[System.Drawing.Bitmap]::new((Join-Path $out ($name+'.png')));$g.DrawImage($b,[System.Drawing.Rectangle]::new($rect[0],$rect[1],$rect[2],$rect[3]));$b.Dispose()}
function Label($g,$s,$x,$y,$size,$color,$bold=$false){$style=if($bold){[System.Drawing.FontStyle]::Bold}else{[System.Drawing.FontStyle]::Regular};$font=[System.Drawing.Font]::new('Malgun Gothic',$size,$style,[System.Drawing.GraphicsUnit]::Pixel);$brush=[System.Drawing.SolidBrush]::new($color);$g.DrawString($s,$font,$brush,[single]$x,[single]$y);$font.Dispose();$brush.Dispose()}
foreach($state in @('PlayerTurn','EnemyTurn')){
 $b=$src.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($b)
 foreach($name in @('TopBar','Button_Map_Normal','Button_Deck_Normal','Res_Panel','Button_EndTurn_Normal','EnemyName_Plate')){$e=$entries | Where-Object name -eq $name;DrawAsset $g $name $e.canonicalRect}
 if($state -eq 'EnemyTurn'){DrawAsset $g 'Button_EndTurn_Disabled' @(1364,817,284,63)}
 # Exactly the canonical item slots; existing item drawings are reference-only game content.
 foreach($r in @(@(1080,10,66,64),@(1165,11,66,62),@(1249,11,64,62))){DrawAsset $g 'ItemSlot_Empty' $r}
 DrawAsset $g 'Icon_SlotPlus' @(1270,31,22,22)
 foreach($r in @(@(1085,15,56,52),@(1170,16,56,51))){$crop=Crop $src $r $r[2] $r[3];$g.DrawImageUnscaled($crop,$r[0],$r[1]);$crop.Dispose()}
 DrawAsset $g 'Icon_Map' @(1493,31,51,52);DrawAsset $g 'Icon_Deck' @(1588,38,46,39)
 DrawAsset $g 'Res_Divider' @(1445,649,206,2);DrawAsset $g 'Res_Divider' @(1445,743,206,2)
 DrawAsset $g 'Res_Slash' @(1553,663,31,70)
 foreach($pair in @(@('Icon_Energy',1450,684,28,40),@('Icon_Ammo',1450,767,24,33))){$iconName=$pair[0];if($state -eq 'EnemyTurn' -and $iconName -eq 'Icon_Energy'){$iconName='Icon_Energy_Disabled'};DrawAsset $g $iconName @($pair[1],$pair[2],$pair[3],$pair[4])}
 DrawAsset $g 'EndTurn_CornerSlash' @(1364,817,47,63)
 $arrowName=if($state -eq 'EnemyTurn'){'Icon_Arrow_Disabled'}else{'Icon_Arrow'};DrawAsset $g $arrowName @(1598,838,33,21)
 foreach($health in @(@('HP',89,826,335,42,65,80),@('EnemyHP',1108,637,266,24,29,40))){
  $prefix=$health[0];$rect=@($health[1],$health[2],$health[3],$health[4]);DrawAsset $g ($prefix+'_Track') $rect
  DrawAsset $g ($prefix+'_Fill') @(($rect[0]+5),($rect[1]+4),[int](($rect[2]-12)*$health[5]/$health[6]),($rect[3]-8));DrawAsset $g ($prefix+'_Frame') $rect
 }
 DrawAsset $g 'Block_Badge' @(38,823,54,61);DrawAsset $g 'EnemyBlock_Badge' @(1110,599,25,33)
 $g.Dispose();$b.Save((Join-Path $out ('Assembly_'+$state+'_TextlessHUD.png')))
 $g=[System.Drawing.Graphics]::FromImage($b);$g.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
 Label $g 'FLOOR' 334 24 12 $white;Label $g '04' 398 17 24 $cyan $true;Label $g 'TURN' 503 24 12 $white;Label $g '1' 554 17 24 $white $true
 Label $g 'B A T T L E   F I E L D' 748 17 11 $white;Label $g '잊혀진 해안' 798 38 13 $white
 Label $g 'WEAPON' 1476 622 10 $white;Label $g '권총' 1550 620 14 $white
 Label $g 'ENERGY' 1447 657 10 $white;Label $g 'AMMO' 1447 750 10 $white
 $num=if($state -eq 'EnemyTurn'){'0'}else{'3'};$numColor=if($state -eq 'EnemyTurn'){[System.Drawing.Color]::FromArgb(90,96,105)}else{$white}
 Label $g $num 1486 669 62 $numColor $true;Label $g '3' 1583 701 25 ([System.Drawing.Color]::FromArgb(138,144,153));Label $g '6' 1512 757 39 $white
 Label $g 'END TURN' 1423 831 27 $numColor
 Label $g '65 / 80' 215 831 24 $dark $true;Label $g '29 / 40' 1212 637 18 $dark $true
 Label $g '1' 50 830 27 $white;Label $g '2' 1138 614 12 $white;Label $g '— 스카웃 유닛' 1125 666 12 $white
 foreach($x in @(451,623,996)){DrawAsset $g 'Hairline_Slash_White' @($x,13,20,42)}
 $g.Dispose();$full=[System.Drawing.Bitmap]::new(1920,1080);$fg=[System.Drawing.Graphics]::FromImage($full);$fg.DrawImage($b,0,0,1920,1080);$fg.Dispose();$full.Save((Join-Path $out ('Example_'+$state+'.png')));$full.Dispose();$b.Dispose()
}
$banner=[System.Drawing.Bitmap]::new((Join-Path $out 'Example_PlayerTurn.png'));$g=[System.Drawing.Graphics]::FromImage($banner)
DrawAsset $g 'Slash_Long' @(872,218,138,205);DrawAsset $g 'Line_Sweep' @(935,355,255,2)
Label $g 'P L A Y E R' 690 275 42 $dark;Label $g 'T U R N' 1010 275 42 $dark;Label $g '아군 턴' 1030 363 19 $dark
$g.Dispose();$banner.Save((Join-Path $out 'Example_TurnBanner.png'));$banner.Dispose()
$manifest=[ordered]@{canonical='BattleHUD_Mockup_v3_Canonical.png';canonicalCanvas=@(1672,941);referenceCanvas=@(1920,1080);assets=$entries;resourceLayers=@(@{name='Res_Panel';rect=@(1435,614,225,192)},@{name='Res_Divider';rect=@(1445,649,206,2)},@{name='Res_Divider';rect=@(1445,743,206,2)},@{name='Res_Slash';rect=@(1553,663,31,70)});buttonIcons=@{Button_Map='Icon_Map';Button_Deck='Icon_Deck';Button_EndTurn='Icon_Arrow'};buttonStates=@{map=@('Normal','Hover','Pressed');deck=@('Normal','Hover','Pressed');endTurn=@('Normal','Hover','Pressed','Disabled')};disabledIcons=@{energy='Icon_Energy_Disabled';arrow='Icon_Arrow_Disabled'};textBandBorders=@(8,4,8,4);smallCyanEnergyDashRemoved=$true;mapSource='BattleHUD_Mockup_v3_Oratio_TurnBanner.png';previewBase='canonical screenshot; gameplay artwork retained'}
$manifest | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$src.Dispose();$skin.Dispose();$map.Dispose()
Write-Output ('Exported '+$entries.Count+' sprites and player/enemy turn examples')
