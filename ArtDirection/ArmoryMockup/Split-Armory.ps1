Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Armory_Equipment_Mockup_v2_Sniper.png'))
$clean=$src.Clone([System.Drawing.Rectangle]::new(0,0,$src.Width,$src.Height),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$repair=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Armory_TextRemoval_Model.png'))
if($repair.Size -ne $src.Size){throw 'Model output size must match source before slicing.'}
# Only these lettering rectangles use generated restoration. Everything else remains source pixels.
$erasures=@(
 @(151,39,35,28),@(228,19,243,80),@(1135,32,205,34),@(1513,37,52,28),
 @(173,126,97,29),@(389,125,105,30),@(113,188,139,40),
 @(128,328,53,32),@(126,464,62,28),@(125,600,157,29),@(126,742,158,29),
 @(168,817,185,60),@(430,184,126,64),@(436,258,91,35),@(464,303,75,28),
 @(436,554,49,69),@(594,553,114,70),@(785,553,99,70),@(446,644,210,28),
 @(431,702,172,32),@(516,755,194,31),@(516,816,194,32),
 @(882,760,109,28),@(882,821,109,28),@(833,870,208,27),
 @(1114,188,150,39),@(1319,198,258,27),@(1318,273,105,64),
 @(1318,388,129,66),@(1316,519,151,65),@(1116,641,89,28),
 @(1120,670,152,49),@(1380,640,107,29),@(1380,669,78,51),
 @(1380,726,146,27),@(1130,805,104,60)
)
$g=[System.Drawing.Graphics]::FromImage($clean)
foreach($e in $erasures){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($repair,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}
if(Test-Path (Join-Path $PSScriptRoot 'Armory_EmptyBacking_Model.png')){
 $badgeRepair=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Armory_EmptyBacking_Model.png'))
 $badgeRect=[System.Drawing.Rectangle]::new(436,258,91,35)
 $g.DrawImage($badgeRepair,$badgeRect,$badgeRect,[System.Drawing.GraphicsUnit]::Pixel);$badgeRepair.Dispose()
}
$g.Dispose();$repair.Dispose()
$clean.Save((Join-Path $out 'Armory_Textless_Master.png'))
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
Cut 'Header_Composite' 85 0 1531 169
Cut 'WeaponList_Composite' 85 169 312 737
Cut 'WeaponDetail_Composite' 397 169 676 737
Cut 'UpgradePanel_Composite' 1086 169 514 737
Cut 'Back_Normal' 1429 27 185 49 $null $true
Cut 'Tab_Equipment_Selected' 114 122 213 37 @(144,122,326,122,296,159,114,159) $true
Cut 'Tab_Catalog_Normal' 308 123 241 36 @(338,123,548,123,548,159,308,159) $true
Cut 'Weapon_Pistol_Selected' 114 247 265 123 $null $true
Cut 'Weapon_SMG_Normal' 114 380 265 121 $null $true
Cut 'Weapon_Sniper_Locked' 114 510 265 129
Cut 'Weapon_Shotgun_Locked' 114 649 265 130
Cut 'Unlock_Disabled' 87 804 293 85 @(98,804,379,804,379,879,369,889,87,889,87,815)
Cut 'CardAttack_Upgrade_Normal' 837 751 197 45 $null $true
Cut 'CardDefense_Upgrade_Normal' 837 812 197 45 $null $true
Cut 'Upgrade_Normal' 1095 774 495 115 @(1116,774,1589,774,1589,846,1545,889,1095,889,1095,796) $true
Cut 'LevelBadge' 436 258 91 35
Cut 'WeaponStats_Composite' 436 540 599 98
Cut 'NextUpgradeStrip' 436 639 600 39
Cut 'BaseCardAttack_Row' 436 742 600 62
Cut 'BaseCardDefense_Row' 436 804 600 62
Cut 'UpgradeTree_Composite' 1209 271 263 314
Cut 'UpgradeNode_Complete' 1217 274 58 58
Cut 'UpgradeNode_Current' 1216 388 60 62
Cut 'UpgradeNode_Locked' 1213 511 67 66
Cut 'UpgradeTree_Connector' 1242 330 7 183
Cut 'UpgradeCost_Composite' 1118 638 455 119
Cut 'ExperienceGauge_Track' 1138 70 252 12
Cut 'ExperienceGauge_Fill' 1138 70 122 12
Cut 'Icon_BackArrow' 1483 41 27 23 $null $false $true
Cut 'Icon_Attack' 452 748 47 51 $null $false $true
Cut 'Icon_Defense' 454 813 43 47 $null $false $true
Cut 'Icon_Lock' 227 541 38 48 $null $false $true
Cut 'Icon_UnlockNotice' 105 826 44 45 $null $false $true
Cut 'Icon_SelectedCheck_Crop' 338 259 29 26
Cut 'Icon_UnlockedCheck_Crop' 435 304 27 27
Cut 'UpgradeArrow_Crop' 1239 809 52 52
Cut 'Pistol_Thumbnail' 175 260 113 96 $null $false $true
Cut 'SMG_Thumbnail' 142 392 211 98 $null $false $true
Cut 'Sniper_LockedThumbnail_Composite' 126 533 239 68
Cut 'Shotgun_LockedThumbnail_Composite' 134 677 227 64
Cut 'WeaponArt_Pistol' 576 248 411 345 @(588,263,603,259,612,252,637,248,646,265,681,270,695,272,843,302,851,302,933,302,939,331,966,330,977,338,976,350,967,357,980,381,980,396,963,399,954,423,963,460,984,558,985,575,951,593,861,577,857,565,866,544,845,447,824,421,759,423,750,417,752,375,731,366,605,358,584,348,576,320,579,281)
Cut 'Divider_Columns' 1069 169 9 737
Cut 'Divider_UpgradeTop' 1118 239 455 4
Cut 'Divider_UpgradeCost' 1118 616 455 5
Cut 'Divider_Cards' 436 693 600 5
Cut 'Decoration_TopLeft' 19 17 42 46 $null $false $true
Cut 'Decoration_TopRight' 1619 18 38 47 $null $false $true
Cut 'PaperTexture' 1000 410 48 64
# Lossless source partitions are for reference, not separate runtime layers.
foreach($r in @(@('Source_Header',0,0,1672,169),@('Source_LeftMargin',0,169,85,737),@('Source_List',85,169,312,737),@('Source_Detail',397,169,676,737),@('Source_Upgrade',1073,169,527,737),@('Source_RightMargin',1600,169,72,737),@('Source_Footer',0,906,1672,35))){Cut $r[0] $r[1] $r[2] $r[3] $r[4] $null $false $false $true}
$controls=@('Back_Normal','Tab_Equipment_Selected','Tab_Catalog_Normal','Weapon_Pistol_Selected','Weapon_SMG_Normal','Weapon_Sniper_Locked','Weapon_Shotgun_Locked','Unlock_Disabled','CardAttack_Upgrade_Normal','CardDefense_Upgrade_Normal','Upgrade_Normal')
$base=$clean.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){if($b.GetPixel($x,$y).A -gt 0){$base.SetPixel(($e.x+$x),($e.y+$y),[System.Drawing.Color]::Transparent)}}};$b.Dispose()}
$base.Save((Join-Path $out 'UI_Base_WithControlHoles.png'))
$assembled=[System.Drawing.Bitmap]::new(1672,941);$g=[System.Drawing.Graphics]::FromImage($assembled);$g.DrawImageUnscaled($base,0,0)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_Textless.png'))
$diff=0;for($y=0;$y -lt 941;$y++){for($x=0;$x -lt 1672;$x++){if($clean.GetPixel($x,$y).ToArgb() -ne $assembled.GetPixel($x,$y).ToArgb()){$diff++}}}
$g=[System.Drawing.Graphics]::FromImage($assembled);foreach($key in $controls){$e=$entries | Where-Object name -eq ($key+'_Hover');if($e){$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()}};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_AllHover.png'))
$backingPath=Join-Path $PSScriptRoot 'Armory_EmptyBacking_Model.png'
if(Test-Path $backingPath){
 $textless=$clean;$clean=[System.Drawing.Bitmap]::new($backingPath)
 if($clean.Size -ne $src.Size){throw 'Backing dimensions mismatch.'}
 Cut 'Backing_FullCanvas_Restored' 0 0 1672 941
 Cut 'Backing_Header_Restored' 85 0 1531 169
 Cut 'Backing_List_Restored' 85 169 312 737
 Cut 'Backing_Detail_Restored' 397 169 676 737
 Cut 'Backing_UpgradePanel_Restored' 1086 169 514 737
 Cut 'Backing_PistolTile_Restored' 114 247 265 123
 Cut 'Backing_SMGTile_Restored' 114 380 265 121
 Cut 'Backing_SniperTile_Restored' 114 510 265 129
 Cut 'Backing_ShotgunTile_Restored' 114 649 265 130
 Cut 'Backing_LevelBadge_Restored' 436 258 91 35
 Cut 'Backing_ExperienceTrack_Restored' 1138 70 252 12
 Cut 'Backing_UpgradeButton_Restored' 1095 774 495 115 @(1116,774,1589,774,1589,846,1545,889,1095,889,1095,796) $true
 Cut 'Backing_CardAttackRow_Restored' 436 742 600 62
 Cut 'Backing_CardDefenseRow_Restored' 436 804 600 62
 $clean.Dispose();$clean=$textless
 foreach($entry in $entries){if($entry.name.StartsWith('Backing_')){$entry.kind='model-restored-backing'}}
}
[ordered]@{canvas=@(1672,941);origin='top-left';source='Armory_Equipment_Mockup_v2_Sniper.png';textEraseRectangles=$erasures;assets=$entries;controlLayers=$controls;textlessReassemblyDifferentPixels=$diff} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$base.Dispose();$assembled.Dispose();$src.Dispose();$clean.Dispose()
Write-Output "Exported $($entries.Count) entries; textless reassembly differing pixels: $diff"
