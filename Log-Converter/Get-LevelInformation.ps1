# Read streamingassets levels.json and output information in a csv: ID, Grabbable Sum, VoxelSize FillableSize
$levelsJsonPath = "Assets\StreamingAssets\levels.json"
$levelsJson = Get-Content -Path $levelsJsonPath -Raw | ConvertFrom-Json
$csvOutputPath = "C:\logs\levelInformation.csv"


$csvContent = @()

$idCounter = 0
foreach ($level in $levelsJson.levels) {
    $grabbableSum = ($level.grabbables | Measure-Object).Count
    $voxelSize = $level.voxelSize
    $fillableSize = $level.fillable.size -join ", "
    $SumOfVoxelsInFillable = $level.fillable.size[0] * $level.fillable.size[1] * $level.fillable.size[2]
    $csvContent += [PSCustomObject]@{
        ID                    = $idCounter
        FarbformSum           = $grabbableSum
        VoxelSize             = $voxelSize
        GitterboxSize         = $fillableSize
        SumOfVoxelsInFillable = $SumOfVoxelsInFillable
    }
    $idCounter++
}
# First entry is removed, as it is not a level
$csvContent = $csvContent | Select-Object -Skip 1
$csvContent | Export-Csv -Path $csvOutputPath -NoTypeInformation