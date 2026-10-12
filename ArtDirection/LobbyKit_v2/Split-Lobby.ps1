Add-Type -AssemblyName System.Drawing
$outDir=Join-Path $PSScriptRoot 'Extracted'
New-Item -ItemType Directory -Force $outDir | Out-Null
$source=[System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot 'Lobby_Textless_Master.png'))
$items=[System.Collections.Generic.List[object]]::new()
function Cut($name,$x,$y,$w,$h,$polygon=$null,$hover=$false,$ink=$false) {
    $bmp=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g=[System.Drawing.Graphics]::FromImage($bmp)
    $g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $path=$null
    if($polygon){
        $path=[System.Drawing.Drawing2D.GraphicsPath]::new()
        $pts=[System.Collections.Generic.List[System.Drawing.PointF]]::new()
        for($n=0;$n -lt $polygon.Count;$n+=2){$pts.Add([System.Drawing.PointF]::new(($polygon[$n]-$x),($polygon[$n+1]-$y)))}
        $path.AddPolygon($pts.ToArray()); $g.SetClip($path)
    }
    $g.DrawImage($source,[System.Drawing.Rectangle]::new(0,0,$w,$h),[System.Drawing.Rectangle]::new($x,$y,$w,$h),[System.Drawing.GraphicsUnit]::Pixel)
    $g.Dispose()
    if($ink){
        for($iy=0;$iy -lt $h;$iy++){for($ix=0;$ix -lt $w;$ix++){
            $c=$bmp.GetPixel($ix,$iy)
            $v=($c.R+$c.G+$c.B)/3
            $a=[int]([Math]::Clamp((220-$v)/160,0,1)*255)
            $bmp.SetPixel($ix,$iy,[System.Drawing.Color]::FromArgb($a,35,37,38))
        }}
    }
    $bmp.Save((Join-Path $outDir ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    $items.Add([pscustomobject]@{name=$name;file=$name+'.png';x=$x;y=$y;width=$w;height=$h;unityAnchor='top-left';unityPosition=@($x,(-$y));kind=$(if($ink){'luminance-extracted icon'}else{'source pixel crop'})})
    if($hover){
        $hg=[System.Drawing.Graphics]::FromImage($bmp)
        if($path){$hg.SetClip($path)}
        $brush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(22,40,206,226))
        $hg.FillRectangle($brush,0,0,$w,$h)
        $pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(220,35,205,226),2)
        if($path){$hg.DrawPath($pen,$path)}else{$hg.DrawRectangle($pen,1,1,($w-3),($h-3))}
        $hg.Dispose();$brush.Dispose();$pen.Dispose()
        $bmp.Save((Join-Path $outDir ($name+'_Hover.png')),[System.Drawing.Imaging.ImageFormat]::Png)
        $items.Add([pscustomobject]@{name=$name+'_Hover';file=$name+'_Hover.png';x=$x;y=$y;width=$w;height=$h;unityAnchor='top-left';unityPosition=@($x,(-$y));kind='same crop plus cyan tint';normal=$name})
    }
    $bmp.Dispose();if($path){$path.Dispose()}
}
Cut 'LeftHeader' 0 0 369 165 @(0,0,368,0,262,107,0,164)
Cut 'LeftLower_Composite' 0 789 224 152 @(0,819,211,789,223,921,151,940,0,940)
Cut 'RightPanel_Composite' 599 0 1073 941 @(1138,0,1671,0,1671,940,599,940,824,715,824,579,840,563,840,410,852,398,852,177)
Cut 'Expedition_Normal' 851 105 785 266 @(925,105,1635,105,1635,349,1614,370,860,370,860,335,851,335,851,176) $true
Cut 'Bonfire_Normal' 840 381 795 183 @(868,381,1634,381,1634,520,1592,563,840,563,840,409) $true
Cut 'Armory_Normal' 830 570 393 135 @(844,570,1222,570,1222,704,830,704,830,584) $true
Cut 'QuestBoard_Normal' 1228 570 407 135 @(1243,570,1634,570,1634,687,1616,704,1228,704,1228,585) $true
Cut 'InformationBroker_Normal' 830 710 393 135 @(830,710,1222,710,1222,826,1205,844,830,844) $true
Cut 'TrainingGround_Normal' 1228 710 407 135 @(1228,710,1634,710,1501,844,1228,844) $true
Cut 'Settings_Button_Normal' 1494 21 48 48 $null $true
Cut 'Settings_Icon' 1504 30 27 29 $null $false $true
Cut 'Brand_Compass' 71 17 50 50 $null $false $true
Cut 'Armory_Icon' 861 609 106 63 $null $false $true
Cut 'QuestBoard_Icon' 1295 604 63 71 $null $false $true
Cut 'InformationBroker_Icon' 886 741 67 74 $null $false $true
Cut 'TrainingGround_Icon' 1286 736 82 82 $null $false $true
Cut 'Expedition_Compass_Crop' 1306 106 290 256
Cut 'Bonfire_Illustration_Crop' 1205 383 374 175
Cut 'Arrow_Dark' 1581 306 31 26 $null $false $true
Cut 'Arrow_Light_Crop' 1580 485 34 32
Cut 'Progress_Composite' 1192 37 200 16
Cut 'Progress_Track_Segment' 1211 40 99 10
Cut 'Progress_Fill_Segment' 1196 42 12 7
Cut 'CornerMark' 1627 96 17 19 $null $false $true
Cut 'Footer_Dark' 1434 704 238 237 @(1671,704,1671,940,1434,940)
Cut 'Header_Right_Composite' 1138 0 534 104
$manifest=[ordered]@{source='Lobby_Textless_Master.png';canvasWidth=1672;canvasHeight=941;coordinateOrigin='top-left';assets=$items}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outDir 'layout.json') -Encoding utf8
# Assemble static UI at source coordinates, then remove replaceable controls.
$static=[System.Drawing.Bitmap]::new(1672,941,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sg=[System.Drawing.Graphics]::FromImage($static)
foreach($key in @('LeftHeader','LeftLower_Composite','RightPanel_Composite')){
    $item=$items | Where-Object name -eq $key
    $part=[System.Drawing.Bitmap]::new((Join-Path $outDir $item.file))
    $sg.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()
}
$sg.Dispose()
$controls=@('Expedition_Normal','Bonfire_Normal','Armory_Normal','QuestBoard_Normal','InformationBroker_Normal','TrainingGround_Normal','Settings_Button_Normal','Progress_Composite')
foreach($key in $controls){
    $item=$items | Where-Object name -eq $key
    $part=[System.Drawing.Bitmap]::new((Join-Path $outDir $item.file))
    for($py=0;$py -lt $part.Height;$py++){for($px=0;$px -lt $part.Width;$px++){
        if($part.GetPixel($px,$py).A -gt 0){$static.SetPixel(($item.x+$px),($item.y+$py),[System.Drawing.Color]::Transparent)}
    }}
    $part.Dispose()
}
$static.Save((Join-Path $outDir 'UI_Static_FullCanvas.png'),[System.Drawing.Imaging.ImageFormat]::Png)
# Retain source scenery+character as an exact reconstruction layer, not a clean background.
$scene=$source.Clone([System.Drawing.Rectangle]::new(0,0,1672,941),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for($py=0;$py -lt 941;$py++){for($px=0;$px -lt 1672;$px++){
    if($static.GetPixel($px,$py).A -gt 0){$scene.SetPixel($px,$py,[System.Drawing.Color]::Transparent)}
}}
foreach($key in $controls){
    $item=$items | Where-Object name -eq $key
    $part=[System.Drawing.Bitmap]::new((Join-Path $outDir $item.file))
    for($py=0;$py -lt $part.Height;$py++){for($px=0;$px -lt $part.Width;$px++){
        if($part.GetPixel($px,$py).A -gt 0){$scene.SetPixel(($item.x+$px),($item.y+$py),[System.Drawing.Color]::Transparent)}
    }}
    $part.Dispose()
}
$scene.Save((Join-Path $outDir 'Scene_WithCharacter_ReferenceOnly.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$preview=[System.Drawing.Bitmap]::new(1672,941)
$pg=[System.Drawing.Graphics]::FromImage($preview)
$pg.DrawImageUnscaled($scene,0,0);$pg.DrawImageUnscaled($static,0,0)
foreach($key in $controls){
    $item=$items | Where-Object name -eq $key
    $part=[System.Drawing.Bitmap]::new((Join-Path $outDir $item.file))
    $pg.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()
}
$pg.Dispose()
$preview.Save((Join-Path $outDir 'Reassembled_Normal.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$mismatch=0
for($py=0;$py -lt 941;$py++){for($px=0;$px -lt 1672;$px++){
    if($preview.GetPixel($px,$py).ToArgb() -ne $source.GetPixel($px,$py).ToArgb()){$mismatch++}
}}
$report=[ordered]@{pixelsDifferentFromMaster=$mismatch;totalPixels=1672*941;layers=@('Scene_WithCharacter_ReferenceOnly.png','UI_Static_FullCanvas.png')+$controls;note='Buttons placed using layout.json; alternate icon and composite crops not stacked.'}
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $outDir 'reassembly-check.json') -Encoding utf8
$pg=[System.Drawing.Graphics]::FromImage($preview)
foreach($key in $controls){
    $item=$items | Where-Object name -eq $key
    $hoverPath=Join-Path $outDir ($key+'_Hover.png')
    if(Test-Path -LiteralPath $hoverPath){
        $part=[System.Drawing.Bitmap]::new($hoverPath)
        $pg.DrawImageUnscaled($part,$item.x,$item.y);$part.Dispose()
    }
}
$pg.Dispose()
$preview.Save((Join-Path $outDir 'Reassembled_AllHover.png'),[System.Drawing.Imaging.ImageFormat]::Png)
foreach($region in @(@('LeftPanel_Static',0,0,369,941),@('RightPanel_Static',599,0,1073,941))){
    $piece=$static.Clone([System.Drawing.Rectangle]::new($region[1],$region[2],$region[3],$region[4]),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $piece.Save((Join-Path $outDir ($region[0]+'.png')),[System.Drawing.Imaging.ImageFormat]::Png);$piece.Dispose()
}
$static.Dispose();$scene.Dispose();$preview.Dispose();$source.Dispose()
Write-Output ('Reassembly differing pixels: '+$mismatch)
Write-Output ('Exported '+$items.Count+' PNG assets')
