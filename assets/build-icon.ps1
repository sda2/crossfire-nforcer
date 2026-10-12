# PNG-to-ICO conversion. Run on the development PC; not required on XP.
Add-Type -AssemblyName System.Drawing
$sourceImage = [System.Drawing.Image]::FromFile((Join-Path $PSScriptRoot 'nforcer-icon.png'))
$iconSizes = @(16,32,48,64)
$entries = @()
try {
    foreach ($size in $iconSizes) {
        $bitmap = New-Object System.Drawing.Bitmap($size,$size,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.DrawImage($sourceImage,0,0,$size,$size)
        $graphics.Dispose()
        $stream = New-Object System.IO.MemoryStream
        $writer = New-Object System.IO.BinaryWriter($stream)
        $stride = [int]([Math]::Ceiling($size / 32.0)*4)
        $writer.Write([int]40); $writer.Write([int]$size); $writer.Write([int]($size*2))
        $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([int]0)
        $writer.Write([int]($size*$size*4+$stride*$size))
        1..4 | ForEach-Object { $writer.Write([int]0) }
        for ($y=$size-1;$y -ge 0;$y--) {
            for ($x=0;$x -lt $size;$x++) {
                $pixel=$bitmap.GetPixel($x,$y)
                $writer.Write([byte]$pixel.B); $writer.Write([byte]$pixel.G)
                $writer.Write([byte]$pixel.R); $writer.Write([byte]$pixel.A)
            }
        }
        for ($y=$size-1;$y -ge 0;$y--) {
            $mask = New-Object byte[] $stride
            for ($x=0;$x -lt $size;$x++) {
                if ($bitmap.GetPixel($x,$y).A -lt 128) {
                    $index=[int][Math]::Floor($x/8)
                    $mask[$index] = $mask[$index] -bor (128 -shr ($x % 8))
                }
            }
            $writer.Write($mask)
        }
        $writer.Flush(); $entries += ,$stream.ToArray()
        $writer.Dispose(); $bitmap.Dispose()
    }
    $outStream=[System.IO.File]::Create((Join-Path $PSScriptRoot 'nforcer.ico'))
    $outWriter=New-Object System.IO.BinaryWriter($outStream)
    try {
        $outWriter.Write([uint16]0); $outWriter.Write([uint16]1); $outWriter.Write([uint16]$iconSizes.Count)
        $offset=6+16*$iconSizes.Count
        for ($i=0;$i -lt $iconSizes.Count;$i++) {
            $outWriter.Write([byte]$iconSizes[$i]); $outWriter.Write([byte]$iconSizes[$i])
            $outWriter.Write([byte]0); $outWriter.Write([byte]0)
            $outWriter.Write([uint16]1); $outWriter.Write([uint16]32)
            $outWriter.Write([int]$entries[$i].Length); $outWriter.Write([int]$offset)
            $offset += $entries[$i].Length
        }
        foreach ($entry in $entries) { $outWriter.Write([byte[]]$entry) }
    } finally { $outWriter.Dispose() }
} finally { $sourceImage.Dispose() }
