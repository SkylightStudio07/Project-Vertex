Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'QuestBoard_Posted_Mockup_v1.png'))
$clean=$src.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$repair=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Quest_Textless_Model.png'))
if($repair.Size -ne $src.Size){throw 'Dimension mismatch'}
$erasures=@(
 @(150,39,40,32),@(231,17,301,80),@(1135,31,207,34),@(1520,39,49,26),
 @(162,127,110,29),@(368,127,117,29),@(590,128,84,28),
 @(108,188,77,38),@(154,266,173,30),@(154,310,173,30),@(154,354,173,30),@(154,399,173,30),
 @(108,469,66,29),@(155,511,50,31),@(155,556,50,31),@(155,600,50,31),@(109,850,145,30),
 @(377,187,157,42),@(1035,194,68,30),@(1165,185,145,43),
 @(427,258,40,30),@(644,260,58,19),@(379,307,185,37),@(379,347,105,27),
 @(478,412,127,30),@(374,482,129,30),@(598,470,112,45),
 @(808,258,42,29),@(758,307,141,38),@(758,347,122,27),@(758,482,129,29),@(1011,488,74,25),
 @(428,559,42,28),@(378,611,110,36),@(378,652,137,26),@(375,778,128,30),@(626,768,84,42),
 @(810,559,43,29),@(1030,559,38,24),@(758,612,112,37),@(758,652,116,27),
 @(830,724,185,29),@(758,778,95,32),@(933,781,151,29),
 @(1165,433,207,38),@(1165,475,160,26),@(1520,439,54,17),
 @(1163,517,375,53),@(1165,594,43,24),@(1200,621,180,25),@(1200,650,120,25),
 @(1165,692,84,27),@(1261,722,189,49),@(1164,790,156,28),@(1464,791,110,27),@(1188,839,134,41),
 @(1456,342,110,76)
)
$g=[System.Drawing.Graphics]::FromImage($clean)
foreach($e in $erasures){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($repair,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}
$backRaw=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Quest_Backing_Model.png'))
$back=[System.Drawing.Bitmap]::new(1672,941);$bg=[System.Drawing.Graphics]::FromImage($back);$bg.DrawImage($backRaw,0,0,1672,941);$bg.Dispose();$backRaw.Dispose()
foreach($e in @(@(1135,31,207,34),@(429,482,77,32),@(810,482,80,32),@(982,488,104,25),@(429,778,77,32),@(1240,790,82,28),@(1187,839,137,42))){$r=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3]);$g.DrawImage($back,$r,$r,[System.Drawing.GraphicsUnit]::Pixel)}
$back.Dispose();$g.Dispose();$repair.Dispose()
$clean.Save((Join-Path $out 'Quest_Textless_Master.png'))
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
Cut 'Filter_Composite' 85 168 263 739
Cut 'List_Composite' 348 168 785 739
Cut 'Detail_Composite' 1147 168 452 739
Cut 'Back_Normal' 1429 27 185 49 $null $true
Cut 'Tab_Posted_Selected' 103 121 226 38 @(132,121,329,121,299,159,103,159) $true
Cut 'Tab_Active_Normal' 307 121 229 38 @(338,121,536,121,504,159,307,159) $true
Cut 'Tab_History_Normal' 516 121 220 38 @(546,121,736,121,706,159,516,159) $true
Cut 'Accept_Normal' 1156 824 431 74 @(1168,824,1587,824,1587,857,1546,898,1156,898,1156,835) $true
Cut 'Card_Selected' 357 238 375 292 $null $true
Cut 'Card_Accepted' 739 240 367 289 $null $true
Cut 'Card_Normal' 359 539 373 287 $null $true
Cut 'Card_Locked' 739 539 367 287
Cut 'Filter_Checked' 110 261 221 36 $null $true
Cut 'Filter_Unchecked' 110 305 221 36 $null $true
Cut 'Checkbox_Checked' 112 264 31 32
Cut 'Checkbox_Unchecked' 112 308 31 32
Cut 'Gauge_ActiveFill' 113 882 85 12
Cut 'Gauge_ExperienceFill' 1138 70 122 12
Cut 'ScrollThumb' 1116 239 12 101
Cut 'ScrollTrack_Composite' 1116 239 12 643
Cut 'Badge_Story_Blank' 620 257 92 26 @(633,257,712,257,712,282,620,282)
Cut 'Badge_Rescue_Blank' 1003 556 87 29 @(1017,556,1090,556,1075,585,1003,585)
Cut 'Icon_Recovery' 382 256 36 34 $null $false $true
Cut 'Icon_Delivery' 763 258 35 29 $null $false $true
Cut 'Icon_Elimination' 382 557 35 34 $null $false $true
Cut 'Icon_Rescue' 763 556 36 35 $null $false $true
Cut 'Icon_Back' 1483 40 28 26 $null $false $true
Cut 'Item_Blackbox_Crop' 379 394 88 72
Cut 'Item_Blackbox_Detail_Crop' 1168 721 79 57
Cut 'Emblem_Drone_Composite' 1158 234 429 191
Cut 'Art_Recovery_Crop' 546 288 177 177
Cut 'Art_Delivery_Crop' 905 311 191 166
Cut 'Art_Elimination_Crop' 523 586 200 175
Cut 'Art_RescueLocked_Crop' 868 593 228 121
Cut 'Divider_Columns' 1128 168 8 739
Cut 'CollectionPanel' 85 838 245 65
# Source reference pieces retain original typography.
Cut 'Source_Header' 0 0 1672 168 $null $false $false $true
Cut 'Source_Body' 0 168 1672 739 $null $false $false $true
Cut 'Source_Footer' 0 907 1672 34 $null $false $false $true
$normal=$clean
foreach($state in @('Disabled','Cancel')){
 $clean=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot ('Quest_'+$state+'_Model.png')))
 if($clean.Size -ne $src.Size){throw 'State dimensions mismatch'}
 $name=if($state -eq 'Cancel'){'Cancel_Normal'}else{'Accept_Disabled'}
 Cut $name 1156 824 431 74 @(1168,824,1587,824,1587,857,1546,898,1156,898,1156,835) ($state -eq 'Cancel')
 $clean.Dispose()
}
$clean=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Quest_Backing_Model.png'))
if($clean.Size -ne $src.Size){$rawBacking=$clean;$clean=[System.Drawing.Bitmap]::new(1672,941);$rg=[System.Drawing.Graphics]::FromImage($clean);$rg.DrawImage($rawBacking,0,0,1672,941);$rg.Dispose();$rawBacking.Dispose()}
Cut 'Backing_FullCanvas_Restored' 0 0 1672 941
Cut 'Backing_Detail_Restored' 1147 168 452 739
Cut 'Backing_Card_Normal_Restored' 359 539 373 287 $null $true
Cut 'Backing_Card_Selected_Restored' 357 238 375 292
Cut 'Backing_Card_Locked_Restored' 739 539 367 287
$clean.Dispose();$clean=$normal
# Cyan-key alpha isolates distressed stamps from grayscale paper without recreating them.
function Stamp($name,$image){
 $b=[System.Drawing.Bitmap]::new(126,82,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 for($y=0;$y -lt 82;$y++){for($x=0;$x -lt 126;$x++){
  $c=$image.GetPixel((975+$x),(240+$y));$a=[int](255*[Math]::Max(0.0,[Math]::Min(1.0,([double]$c.B-[double]$c.R)/229.0)))
  if($a -lt 8){$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(0,0,0,0))}else{$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($a,13,184,242))}
 }}
 $b.Save((Join-Path $out ($name+'.png')));$b.Dispose()
 $entries.Add([pscustomobject]@{name=$name;file=$name+'.png';x=975;y=240;width=126;height=82;kind='cyan-isolated-alpha-stamp'})
}
Stamp 'Stamp_Accepted_KO' $src
$completed=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Quest_Complete_Model.png'))
Stamp 'Stamp_Completed_KO' $completed
$completed.Dispose()
# Empty stamp frame for localized runtime labels.
$blank=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Quest_Textless_Model.png'))
Stamp 'Stamp_Frame_Textless' $blank
$blank.Dispose()
# Derive alternate tab fills from the same cropped silhouettes, preserving alpha.
foreach($spec in @(@('Tab_Posted_Selected','Tab_Posted_Normal',$false),@('Tab_Active_Normal','Tab_Active_Selected',$true),@('Tab_History_Normal','Tab_History_Selected',$true))){
 $e=$entries | Where-Object name -eq $spec[0]
 $b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file))
 for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){
  $c=$b.GetPixel($x,$y);if($c.A -gt 0){if($spec[2]){$r=13;$gg=184;$bb=242}else{$r=232;$gg=233;$bb=234};$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($c.A,$r,$gg,$bb))}
 }}
 $b.Save((Join-Path $out ($spec[1]+'.png')))
 if($spec[1] -eq 'Tab_Posted_Normal'){
  for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){$c=$b.GetPixel($x,$y);if($c.A -gt 0){$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($c.A,195,231,240))}}}
  $b.Save((Join-Path $out 'Tab_Posted_Normal_Hover.png'))
  $entries.Add([pscustomobject]@{name='Tab_Posted_Normal_Hover';file='Tab_Posted_Normal_Hover.png';x=$e.x;y=$e.y;width=$e.width;height=$e.height;kind='hover';normal='Tab_Posted_Normal'})
 }
 $b.Dispose()
 $entries.Add([pscustomobject]@{name=$spec[1];file=$spec[1]+'.png';x=$e.x;y=$e.y;width=$e.width;height=$e.height;kind='same-silhouette-tab-state'})
}
$controls=@('Back_Normal','Tab_Posted_Selected','Tab_Active_Normal','Tab_History_Normal','Accept_Normal','Card_Selected','Card_Accepted','Card_Normal','Card_Locked')
$base=$clean.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));for($y=0;$y -lt $b.Height;$y++){for($x=0;$x -lt $b.Width;$x++){if($b.GetPixel($x,$y).A -gt 0){$base.SetPixel(($e.x+$x),($e.y+$y),[System.Drawing.Color]::Transparent)}}};$b.Dispose()}
$base.Save((Join-Path $out 'UI_Base_WithControlHoles.png'))
$assembled=[System.Drawing.Bitmap]::new(1672,941);$g=[System.Drawing.Graphics]::FromImage($assembled);$g.DrawImageUnscaled($base,0,0)
foreach($key in $controls){$e=$entries | Where-Object name -eq $key;$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_Normal.png'))
$diff=0;for($y=0;$y -lt 941;$y++){for($x=0;$x -lt 1672;$x++){if($clean.GetPixel($x,$y).ToArgb() -ne $assembled.GetPixel($x,$y).ToArgb()){$diff++}}}
foreach($state in @('Accept_Disabled','Cancel_Normal')){
 $preview=$clean.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($preview);$b=[System.Drawing.Bitmap]::new((Join-Path $out ($state+'.png')));$g.DrawImageUnscaled($b,1156,824);$g.Dispose();$b.Dispose();$preview.Save((Join-Path $out ('Preview_'+$state+'.png')));$preview.Dispose()
}
$g=[System.Drawing.Graphics]::FromImage($assembled);foreach($key in $controls){$e=$entries | Where-Object name -eq ($key+'_Hover');if($e){$b=[System.Drawing.Bitmap]::new((Join-Path $out $e.file));$g.DrawImageUnscaled($b,$e.x,$e.y);$b.Dispose()}};$g.Dispose();$assembled.Save((Join-Path $out 'Reassembled_AllHover.png'))
[ordered]@{canvas=@(1672,941);origin='top-left';source='QuestBoard_Posted_Mockup_v1.png';assets=$entries;controlLayers=$controls;erasures=$erasures;reassemblyDifferentPixels=$diff;actionStates=@{normal='Accept_Normal';hover='Accept_Normal_Hover';disabled='Accept_Disabled';cancel='Cancel_Normal';cancelHover='Cancel_Normal_Hover';rect=@(1156,824,431,74)}} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$base.Dispose();$assembled.Dispose();$src.Dispose();$clean.Dispose()
Write-Output "Exported $($entries.Count) images; reassembly differing pixels: $diff"
