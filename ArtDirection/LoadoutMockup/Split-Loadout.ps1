Add-Type -AssemblyName System.Drawing
$out=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $out | Out-Null
$src=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Loadout_Mockup_v1.png'))
$clean=$src.Clone([System.Drawing.Rectangle]::new(0,0,$src.Width,$src.Height),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
# Remove only lettering using nearby surface samples. Geometry/artwork outside these boxes is untouched.
$erasures=@(
 @(150,39,40,33,175,76),@(226,15,254,89,483,16),@(1515,37,67,29,1559,29),
 @(166,148,71,29,143,159),@(119,408,173,81,284,430),@(122,497,183,35,310,505),
 @(120,577,52,70,180,579),@(279,576,62,71,343,579),@(431,575,111,72,570,560),
 @(126,679,142,27,303,680),
 @(1094,130,184,36,1281,143),@(1094,166,203,23,1308,163),
 @(1201,217,174,65,1363,214),@(1490,234,53,39,1450,226),
 @(1200,313,175,62,1363,313),@(1491,331,55,41,1450,327),
 @(1202,408,107,66,1363,409),@(1490,427,54,39,1450,425),
 @(1458,496,109,25,1340,496),@(1094,562,163,37,1280,565),
 @(1293,653,154,36,1458,636),
 @(199,844,54,25,284,841),@(415,843,59,26,485,842),
 @(640,842,42,27,585,841),@(691,845,60,22,585,841),
 @(811,842,70,26,798,842),@(883,845,65,23,932,820),
 @(1123,777,108,26,1233,782),@(1120,803,100,55,1300,815)
)
$cg=[System.Drawing.Graphics]::FromImage($clean)
$repair=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Loadout_TextRemoval_Model.png'))
foreach($e in $erasures){
    $rect=[System.Drawing.Rectangle]::new($e[0],$e[1],$e[2],$e[3])
    $cg.DrawImage($repair,$rect,$rect,[System.Drawing.GraphicsUnit]::Pixel)
}
$cg.Dispose();$repair.Dispose()
$clean.Save((Join-Path $out 'Loadout_Textless_Master.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$entries=[System.Collections.Generic.List[object]]::new()
function Cut($name,$x,$y,$w,$h,$poly=$null,$interactive=$false,$ink=$false,$reference=$false){
    $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g=[System.Drawing.Graphics]::FromImage($b);$g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $path=$null
    if($poly){
        $path=[System.Drawing.Drawing2D.GraphicsPath]::new();$pts=[System.Collections.Generic.List[System.Drawing.PointF]]::new()
        for($n=0;$n -lt $poly.Count;$n+=2){$pts.Add([System.Drawing.PointF]::new(($poly[$n]-$x),($poly[$n+1]-$y)))}
        $path.AddPolygon($pts.ToArray());$g.SetClip($path)
    }
    $image=if($reference){$src}else{$clean}
    $g.DrawImage($image,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($x,$y,$w,$h),[System.Drawing.GraphicsUnit]::Pixel);$g.Dispose()
    if($ink){for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){
        $c=$b.GetPixel($ix,$iy);$v=($c.R+$c.G+$c.B)/3.0
        $a=[int]([Math]::Clamp((220.0-$v)/160.0,0.0,1.0)*$c.A)
        $b.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb($a,32,34,35))
    }}}
    $b.Save((Join-Path $out ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    $entries.Add([pscustomobject]@{name=$name;file=$name+'.png';x=$x;y=$y;width=$w;height=$h;position=@($x,(-$y));kind=$(if($reference){'source with text'}elseif($ink){'luminance alpha icon'}else{'textless crop'})})
    if($interactive){
        $hg=[System.Drawing.Graphics]::FromImage($b);if($path){$hg.SetClip($path)}
        $brush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(20,30,205,226));$hg.FillRectangle($brush,0,0,$w,$h)
        $pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(230,20,200,225),2)
        if($path){$hg.DrawPath($pen,$path)}else{$hg.DrawRectangle($pen,1,1,($w-3),($h-3))}
        $hg.Dispose();$pen.Dispose();$brush.Dispose()
        $b.Save((Join-Path $out ($name+'_Hover.png')),[System.Drawing.Imaging.ImageFormat]::Png)
        $entries.Add([pscustomobject]@{name=$name+'_Hover';file=$name+'_Hover.png';x=$x;y=$y;width=$w;height=$h;position=@($x,(-$y));kind='cyan hover, exact normal geometry';normal=$name})
    }
    $b.Dispose();if($path){$path.Dispose()}
}
# Major review composites, not stacked with their child slices.
Cut 'Header_Composite' 85 0 1531 115
Cut 'WeaponPanel_Composite' 79 114 971 791
Cut 'DeckPanel_Composite' 1049 114 566 791
Cut 'WeaponShowcase_Composite' 123 116 878 570
Cut 'WeaponStats' 123 552 444 117
Cut 'DeckList_Composite' 1098 202 472 287
Cut 'DeckRow_Attack' 1098 202 472 96
Cut 'DeckRow_Defense' 1098 297 472 97
Cut 'DeckRow_Reload' 1098 393 472 96
Cut 'PerkSection_Composite' 1098 545 474 182
# Independently replaceable controls.
Cut 'Back_Normal' 1429 27 185 49 $null $true
Cut 'SelectedTag' 124 146 157 34 @(157,146,280,146,247,179,124,179)
Cut 'ArmoryLink_Normal' 125 677 166 36 $null $true
Cut 'Weapon_Pistol_Normal' 123 742 210 134 $null $true
Cut 'Weapon_SMG_Normal' 338 742 210 134 $null $true
Cut 'Weapon_AR_Locked' 553 742 210 134
Cut 'Weapon_Shotgun_Locked' 768 742 210 134
Cut 'Carousel_Previous_Normal' 68 783 37 52 $null $true
Cut 'Carousel_Next_Normal' 997 783 37 52 $null $true
Cut 'PerkSlot_Locked' 1100 613 472 114 @(1118,613,1571,613,1571,705,1550,726,1100,726,1100,632)
Cut 'Deploy_Normal' 1076 755 515 129 @(1108,755,1590,755,1590,834,1540,883,1076,883,1076,788) $true
# Symbols and artwork crops.
Cut 'Icon_Attack' 1114 222 59 59 $null $false $true
Cut 'Icon_Defense' 1120 317 51 58 $null $false $true
Cut 'Icon_Reload' 1120 415 52 51 $null $false $true
Cut 'Icon_Lock' 1237 646 36 44 $null $false $true
Cut 'Icon_BackArrow' 1482 40 27 25 $null $false $true
Cut 'Icon_Previous' 78 792 17 36 $null $false $true
Cut 'Icon_Next' 1005 792 18 36 $null $false $true
Cut 'DeployArrow_Crop' 1235 809 52 43
Cut 'Pistol_Thumbnail_Crop' 175 753 98 81
Cut 'SMG_Thumbnail_Crop' 370 750 148 77
Cut 'AR_Locked_Thumbnail_Crop' 575 757 173 78
Cut 'Shotgun_Locked_Thumbnail_Crop' 787 757 170 78
Cut 'WeaponArt_Pistol' 294 177 602 489 @(319,177,361,180,371,197,635,244,788,260,828,270,864,289,864,312,848,331,877,356,878,378,828,412,850,496,878,550,895,644,871,665,713,639,710,613,718,585,676,460,650,422,584,415,551,382,549,363,519,347,339,311,307,293,294,251,294,209)
Cut 'Decoration_TopLeft' 22 18 36 44 $null $false $true
Cut 'Decoration_TopRight' 1621 22 34 43 $null $false $true
Cut 'Decoration_BottomLeft' 20 890 37 35 $null $false $true
Cut 'Divider_Deck' 1097 541 480 8
Cut 'Divider_Columns' 1045 112 9 793
Cut 'Paper_Texture' 925 400 64 64
# Exact rectangular source partition for lossless reassembly verification.
foreach($r in @(@('Source_Header',0,0,1672,114),@('Source_LeftMargin',0,114,79,791),@('Source_Weapon',79,114,970,791),@('Source_Deck',1049,114,566,791),@('Source_RightMargin',1615,114,57,791),@('Source_Footer',0,905,1672,36))){
    Cut $r[0] $r[1] $r[2] $r[3] $r[4] $null $false $false $true
}
$canvas=[System.Drawing.Bitmap]::new(1672,941);$ag=[System.Drawing.Graphics]::FromImage($canvas)
foreach($item in ($entries | Where-Object kind -eq 'source with text')){
    $part=[System.Drawing.Bitmap]::new((Join-Path $out $item.file));$ag.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()
}
$ag.Dispose();$canvas.Save((Join-Path $out 'Reassembled_Source.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$diff=0
for($py=0;$py -lt 941;$py++){for($px=0;$px -lt 1672;$px++){if($src.GetPixel($px,$py).ToArgb() -ne $canvas.GetPixel($px,$py).ToArgb()){$diff++}}}
$savedClean=$clean
$clean=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Loadout_EmptyBacking_Model.png'))
foreach($r in @(@('Backing_Header_Restored',85,0,1531,114),@('Backing_Weapon_Restored',79,114,970,791),@('Backing_Deck_Restored',1049,114,566,791),@('Backing_WeaponShowcase_Restored',123,181,878,502),@('Backing_DeckRow_Restored',1098,202,472,96),@('Backing_PistolTile_Restored',123,742,210,134),@('Backing_SMGTile_Restored',338,742,210,134),@('Backing_ARTile_Restored',553,742,210,134),@('Backing_ShotgunTile_Restored',768,742,210,134),@('Backing_PerkSlot_Restored',1100,613,472,114))){
    Cut $r[0] $r[1] $r[2] $r[3] $r[4]
}
Cut 'Backing_Deploy_Restored' 1076 755 515 129 @(1108,755,1590,755,1590,834,1540,883,1076,883,1076,788) $true
$clean.Dispose();$clean=$savedClean
# Assembly base: textless original with independent controls punched out.
$base=$clean.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$controls=@('Back_Normal','SelectedTag','ArmoryLink_Normal','Weapon_Pistol_Normal','Weapon_SMG_Normal','Weapon_AR_Locked','Weapon_Shotgun_Locked','Carousel_Previous_Normal','Carousel_Next_Normal','PerkSlot_Locked','Deploy_Normal')
foreach($key in $controls){
    $item=$entries | Where-Object name -eq $key
    $part=[System.Drawing.Bitmap]::new((Join-Path $out $item.file))
    for($py=0;$py -lt $part.Height;$py++){for($px=0;$px -lt $part.Width;$px++){
        if($part.GetPixel($px,$py).A -gt 0){$base.SetPixel(($item.x+$px),($item.y+$py),[System.Drawing.Color]::Transparent)}
    }};$part.Dispose()
}
$base.Save((Join-Path $out 'UI_Base_WithControlHoles.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$assembled=[System.Drawing.Bitmap]::new(1672,941);$gg=[System.Drawing.Graphics]::FromImage($assembled);$gg.DrawImageUnscaled($base,0,0)
foreach($key in $controls){$item=$entries | Where-Object name -eq $key;$part=[System.Drawing.Bitmap]::new((Join-Path $out $item.file));$gg.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()}
$gg.Dispose();$assembled.Save((Join-Path $out 'Reassembled_Textless.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$cleanDiff=0
for($py=0;$py -lt 941;$py++){for($px=0;$px -lt 1672;$px++){if($clean.GetPixel($px,$py).ToArgb() -ne $assembled.GetPixel($px,$py).ToArgb()){$cleanDiff++}}}
$gg=[System.Drawing.Graphics]::FromImage($assembled)
foreach($key in $controls){$item=$entries | Where-Object name -eq $key;$p=Join-Path $out ($key+'_Hover.png');if(Test-Path $p){$part=[System.Drawing.Bitmap]::new($p);$gg.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()}}
$gg.Dispose();$assembled.Save((Join-Path $out 'Reassembled_AllHover.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$base.Dispose();$assembled.Dispose()
$manifest=[ordered]@{canvas=@(1672,941);origin='top-left';textEraseRectangles=$erasures;assets=$entries;sourceReassemblyDifferentPixels=$diff;textlessReassemblyDifferentPixels=$cleanDiff;controlLayers=$controls}
$manifest | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $out 'layout.json') -Encoding utf8
$canvas.Dispose();$src.Dispose();$clean.Dispose()
Write-Output ('Exported '+$entries.Count+' sprites; source reconstruction differing pixels: '+$diff)
