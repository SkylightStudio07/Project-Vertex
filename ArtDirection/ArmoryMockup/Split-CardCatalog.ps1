Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'CardCatalogExtracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Armory_CardCatalog_Mockup_v1.png'))
$clean=$src.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$repair=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Catalog_Textless_Model.png'))
if($repair.Size -ne $src.Size){throw 'Image dimensions mismatch'}
$erasures=@(
 @(151,39,35,28),@(228,19,267,80),@(1135,32,205,34),@(1513,37,52,28),
 @(173,126,97,29),@(389,125,105,30),@(110,187,75,39),@(110,257,61,30),
 @(152,301,177,152),@(110,488,64,30),@(154,530,56,109),@(110,669,62,40),@(154,717,58,106),
 @(110,849,142,31),@(378,186,154,41),@(1027,195,79,31),
 @(1165,185,144,44),@(1193,469,132,70),@(1170,564,145,69),@(1170,660,138,58),@(1170,740,158,58),
 @(1183,838,139,43)
)
foreach($x in @(380,565,750,935)){
 $erasures+= ,@(($x+10),410,150,29)
 $erasures+= ,@(($x+10),471,150,42)
 $erasures+= ,@(($x+51),522,69,13)
 $erasures+= ,@(($x+10),717,150,29)
 $erasures+= ,@(($x+10),781,150,42)
 $erasures+= ,@(($x+51),831,69,13)
}
$erasures+= ,@(628,622,40,46)
$erasures+= ,@(813,622,40,46)
$g=[System.Drawing.Graphics]::FromImage($clean)
foreach($e in $erasures){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($repair,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}
$plain=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Catalog_Backing_Model.png'))
foreach($e in $erasures){if($e[0] -ge 1160 -and $e[1] -ge 469){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($plain,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}}
$plain.Dispose()
$g.Dispose();$repair.Dispose()
$clean.Save((Join-Path $out 'Catalog_Textless_Master.png'))
$entries=[System.Collections.Generic.List[object]]::new()
function Cut($name,$x,$y,$w,$h,$poly=$null,$hover=$false,$ink=$false,$source=$false){
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($b);$g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
 $path=$null
 if($poly){$path=[System.Drawing.Drawing2D.GraphicsPath]::new();$pts=[System.Collections.Generic.List[System.Drawing.PointF]]::new();for($i=0;$i -lt $poly.Count;$i+=2){$pts.Add([System.Drawing.PointF]::new(($poly[$i]-$x),($poly[$i+1]-$y)))};$path.AddPolygon($pts.ToArray());$g.SetClip($path)}
 $im=if($source){$src}else{$clean}
 $g.DrawImage($im,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($x,$y,$w,$h),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose()
 if($ink){for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){$c=$b.GetPixel($ix,$iy);$a=[int]([Math]::Max(0,[Math]::Min(1,(235-($c.R+$c.G+$c.B)/3)/205))*$c.A);$b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb($a,29,31,32))}}}
 $b.Save((Join-Path $out ($name+'.png')))
 $entries.Add([pscustomobject]@{name=$name;file=$name+'.png';x=$x;y=$y;width=$w;height=$h;kind=$(if($source){'source-reference'}elseif($ink){'luminance-alpha'}else{'textless-crop'})})
 if($hover){$hg=[System.Drawing.Graphics]::FromImage($b);if($path){$hg.SetClip($path)};$brush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(20,20,205,225));$pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(235,0,202,227),2);$hg.FillRectangle($brush,0,0,$w,$h);if($path){$hg.DrawPath($pen,$path)}else{$hg.DrawRectangle($pen,1,1,($w-3),($h-3))};$hg.Dispose();$brush.Dispose();$pen.Dispose();$b.Save((Join-Path $out ($name+'_Hover.png')));$entries.Add([pscustomobject]@{name=$name+'_Hover';file=$name+'_Hover.png';x=$x;y=$y;width=$w;height=$h;kind='hover';normal=$name})}
 $b.Dispose();if($path){$path.Dispose()}
}

Cut 'Header_Composite' 85 0 1531 168
Cut 'Filters_Composite' 85 168 263 739
Cut 'Grid_Composite' 348 168 785 739
Cut 'Detail_Composite' 1147 168 452 739
Cut 'Back_Normal' 1429 27 185 49 $null $true
Cut 'Tab_Equipment_Normal' 105 122 222 37 @(134,122,327,122,296,159,105,159) $true
Cut 'Tab_Catalog_Selected' 307 122 242 37 @(338,122,549,122,549,159,307,159) $true
Cut 'Upgrade_Normal' 1157 808 428 90 @(1176,808,1585,808,1585,853,1540,898,1157,898,1157,828) $true
Cut 'Checkbox_Selected' 114 301 29 29 $null $true
Cut 'Checkbox_Unchecked' 114 342 29 29 $null $true
Cut 'FilterRow_Selected' 110 297 220 36 $null $true
Cut 'FilterRow_Normal' 110 338 220 37 $null $true
Cut 'CollectionPanel' 86 840 244 63 @(96,840,330,840,330,903,86,903,86,851)
Cut 'CollectionGauge_Fill' 114 882 125 11
Cut 'ExperienceGauge_Fill' 1138 70 122 12
Cut 'ScrollThumb' 1115 242 10 83
Cut 'ScrollTrack_Composite' 1115 242 10 644
Cut 'DetailArt_Composite' 1194 240 358 222
Cut 'DetailComparison_Composite' 1168 552 411 96
Cut 'DetailUnlock_Composite' 1168 648 411 82
Cut 'DetailUpgrade_Composite' 1168 730 411 78
Cut 'Icon_BackArrow' 1481 40 29 25 $null $false $true
Cut 'UpgradeArrow_Crop' 1327 839 41 40
Cut 'Lock_Crop' 628 674 37 39
Cut 'Energy_Cyan' 393 439 22 23
Cut 'Energy_Gray' 418 439 22 23
Cut 'Divider_Left' 345 168 7 739
Cut 'Divider_Right' 1130 168 6 739
Cut 'Decoration_TopLeft' 19 17 42 46 $null $false $true
$names=@('Attack','Defense','Burst','Sniper','Reload','AffinityLocked','RewardLocked','HotReload')
$controls=@('Back_Normal','Tab_Equipment_Normal','Tab_Catalog_Selected','Upgrade_Normal')
for($i=0;$i -lt 8;$i++){
 $x=@(378,563,748,933)[$i%4];$y=if($i -lt 4){242}else{553}
 $locked=$i -eq 5 -or $i -eq 6
 $n='Card_'+$names[$i]
 Cut $n $x $y 172 301 $null (!$locked)
 Cut ('Art_'+$names[$i]+'_Crop') ($x+8) ($y+9) 155 153
 $controls+=$n
}
Cut 'Source_Header' 0 0 1672 168 $null $false $false $true
Cut 'Source_Body' 0 168 1672 739 $null $false $false $true
Cut 'Source_Footer' 0 907 1672 34 $null $false $false $true
$normal=$clean
$clean=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Catalog_Disabled_Model.png'))
if($clean.Size -ne $src.Size){throw 'Disabled dimensions mismatch'}
Cut 'Upgrade_Disabled' 1157 808 428 90 @(1176,808,1585,808,1585,853,1540,898,1157,898,1157,828)
$clean.Dispose();$clean=$normal
$disabled=$normal.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[System.Drawing.Graphics]::FromImage($disabled);$b=[System.Drawing.Bitmap]::new((Join-Path $out 'Upgrade_Disabled.png'));$g.DrawImageUnscaled($b,1157,808);$b.Dispose();$g.Dispose()
$disabled.Save((Join-Path $out 'Preview_Disabled_Textless.png'));$disabled.Dispose()
$clean=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Catalog_Backing_Model.png'))
if($clean.Size -ne $src.Size){throw 'Backing dimensions mismatch'}
Cut 'Backing_FullCanvas_Restored' 0 0 1672 941
Cut 'Backing_Filter_Restored' 85 168 263 739
Cut 'Backing_Grid_Restored' 348 168 785 739
Cut 'Backing_Detail_Restored' 1147 168 452 739
Cut 'Backing_Upgrade_Normal' 1157 808 428 90 @(1176,808,1585,808,1585,853,1540,898,1157,898,1157,828) $true
$clean.Dispose();$clean=$normal
$base=$clean.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){if($b.GetPixel($x,$y).A -gt 0){$base.SetPixel(($e.x+$x),($e.y+$y),[System.Drawing.Color]::Transparent)}}};$b.Dispose()}
$base.Save((Join-Path $out 'UI_Base_WithControlHoles.png'))
$assembled=[System.Drawing.Bitmap]::new(1672,941);$g=[System.Drawing.Graphics]::FromImage($assembled);$g.DrawImageUnscaled($base,0,0)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_Textless.png'))
$diff=0;for($y=0;$y -lt 941;$y++){for($x=0;$x -lt 1672;$x++){if($clean.GetPixel($x,$y).ToArgb() -ne $assembled.GetPixel($x,$y).ToArgb()){$diff++}}}
$g=[System.Drawing.Graphics]::FromImage($assembled);foreach($key in $controls){$e=$entries | Where-Object name -eq ($key+'_Hover');if($e){$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()}};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_AllHover.png'))
[ordered]@{canvas=@(1672,941);origin='top-left';source='Armory_CardCatalog_Mockup_v1.png';assets=$entries;controlLayers=$controls;erasures=$erasures;textlessReassemblyDifferentPixels=$diff;upgradeStates=@{normal='Upgrade_Normal';hover='Upgrade_Normal_Hover';disabled='Upgrade_Disabled';rect=@(1157,808,428,90);disabledInteractable=$false;disabledLabel='강화 불가';disabledHint='무기고 강화 대상이 아닙니다'}} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$base.Dispose();$assembled.Dispose();$src.Dispose();$clean.Dispose()
Write-Output "Exported $($entries.Count) assets; differing reassembly pixels: $diff"
