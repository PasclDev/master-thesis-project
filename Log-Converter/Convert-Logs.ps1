# File that converts the log files into a singular CSV file
$logsPath = "C:\logs\"
$targetPath = "C:\logs\combined_logs.csv"
# File name example: "Log_TP23_F2.csv"

# Example log file: 
# TimeSinceStart;TutorialStep;LevelID;NumberOfFillables;NumberOfGrabbables;LevelCompleted;TimeToComplete;TimeTilFirstGrab;NumberOfGrabs;NumberOfSnapsToFillables;NumberOfFillableTransparency;NumberOfGrabbableTransparency
# 19,227590;0;0;0;0;True;19,227590;19,227590;1;0;0;0
# 77,366620;1;0;0;0;True;58,139030;0,097227;10;0;0;0
# 85,824820;2;0;0;0;True;8,458199;5,735550;1;1;2;1
# 97,005500;3;0;0;0;True;11,180680;4,458794;2;1;1;0
# 121,977700;4;0;0;0;True;24,972240;0,000000;0;0;0;0
# 127,713800;5;0;0;0;True;5,736069;0,000000;0;0;2;0
# 242,408400;6;0;0;0;True;114,694600;1,555038;13;1;9;0
# 267,186500;0;1;1;2;True;24,778050;4,666656;5;3;0;0
# 411,686300;0;2;1;2;True;144,499800;8,277100;34;4;1;0
# 438,422400;0;3;1;2;True;26,736110;4,291992;7;4;0;0
# 483,533100;0;4;1;6;True;45,110720;2,735565;11;7;0;0
# 504,020100;0;5;1;3;True;20,486910;2,791504;5;3;0;0
# 548,145800;0;6;1;4;True;44,125760;1,888428;12;4;0;0
# 566,840100;0;7;1;4;True;18,694340;6,318298;4;4;0;0
# 1149,473000;0;8;1;6;True;582,632400;1,319397;170;41;8;3
# 1720,474000;0;10;1;9;False;571,001500;6,361328;172;49;23;1

# Turns all information into one CSV entry, where every line depends on either the TutorialStep or the LevelID, like so:
# TimeSinceStart;TutorialStep;LevelID;NumberOfFillables;NumberOfGrabbables;LevelCompleted;TimeToComplete;TimeTilFirstGrab;NumberOfGrabs;NumberOfSnapsToFillables;NumberOfFillableTransparency;NumberOfGrabbableTransparency
# 19,227590;0;0;0;0;True;19,227590;19,227590;1;0;0;0
# turns into:
# ID, 0_0_TimeSinceStart, 0_0_TutorialStep, 0_0_LevelID, 0_0_NumberOfFillables, 0_0_NumberOfGrabbables, 0_0_LevelCompleted, 0_0_TimeToComplete, 0_0_TimeTilFirstGrab, 0_0_NumberOfGrabs, 0_0_NumberOfSnapsToFillables, 0_0_NumberOfFillableTransparency, 0_0_NumberOfGrabbableTransparency
# where ID is the number after TP in the file name, and the rest are the values from the log file. 0_0 because the tutorial step is 0 and the level ID is 0.
# the final colums depend on the number of lines in the log file, so it is not possible to create a static CSV file with all the columns.



# Gets all log files
$logFiles = Get-ChildItem -Path $logsPath -Filter "Log_TP*.csv" | Sort-Object Name

Write-Output "Found $($logFiles.Count) log files."
$csvData = @()
foreach ($logFile in $logFiles) {
    $id = $logFile.Name -replace 'Log_TP(\d+)_F2.csv', '$1'

    # Read the log file and convert it to a CSV format and replace the comma with a dot in the numbers so calculations work better (US standard to use dots)
    $logData = Import-Csv -Path $logFile.FullName -Delimiter ';' | ForEach-Object {
        $_ | ForEach-Object { 
            foreach ($property in $_.PSObject.Properties) {
                if ($property.Value -match '^\d+,\d+$') {
                    $property.Value = $property.Value -replace ',', '.'
                }
            }
            $_
        }
    }
    $testingTime = ($logData | Select-Object -Last 1).TimeSinceStart
    # If TimeSinceStart of the last and the second to last line are the same, set LevelCompleted of the second to last line to false, as it wrongfully got flagged as completed
    if ($logData.Count -gt 1) {
        $lastLine = $logData | Select-Object -Last 1
        $secondToLastLine = $logData | Select-Object -Last 2 | Select-Object -First 1
        if ($lastLine.TimeSinceStart -eq $secondToLastLine.TimeSinceStart) {
            $secondToLastLine.LevelCompleted = 'False'
        }
    }
    # Combination of TutorialStep and LevelID should be unique, so remove duplicates. If there are duplicates, remove the one where LevelCompleted is false, if there are still more than one, keep the first one
    # Removes all duplicates and where LevelCompleted is false
    $seen = @{}
    $logData = $logData | Where-Object {
        $key = "$($_.TutorialStep)_$($_.LevelID)"
        if (-not $seen.ContainsKey($key)) {
            $seen[$key] = $_.LevelCompleted -eq 'True'
            $_.LevelCompleted -eq 'True'
        }
        elseif ($seen[$key] -eq $false -and $_.LevelCompleted -eq 'True') {
            $seen[$key] = $true
            $true
        }
        else {
            $false
        }
    }
    Write-Output "Adding $($logFile.Name) with $($logData.Count) non-duplicate & completed level lines to CSV."
    $levelsCompleted = $logData | Where-Object { $_.LevelCompleted -eq 'True' -and $_.LevelID -ne 0 } | Measure-Object | Select-Object -ExpandProperty Count
    # Check if an entry for TutorialStep 6 exists, as well as for LevelID 4, 10, 13, 16
    $isTutorialComplete = ($logData | Where-Object { $_.TutorialStep -eq 6 -and $_.LevelID -eq 0 } | Measure-Object | Select-Object -ExpandProperty Count) -gt 0
    $isFirstLevelPairCompleted = ($logData | Where-Object { $_.LevelID -in 4, 13 } | Measure-Object | Select-Object -ExpandProperty Count) -eq 2
    $isSecondLevelPairCompleted = ($logData | Where-Object { $_.LevelID -in 10, 16 } | Measure-Object | Select-Object -ExpandProperty Count) -eq 2
    $csvLine = [ordered]@{
        ID                                   = $id
        LevelsCompleted                      = $levelsCompleted
        IsTutorialComplete                   = $isTutorialComplete
        IsFirstLevelPairCompleted            = $isFirstLevelPairCompleted
        IsSecondLevelPairCompleted           = $isSecondLevelPairCompleted
        TestingTime                          = $testingTime
        AverageTimeToComplete                = ($logData | Measure-Object -Property TimeToComplete -Sum).Sum / $logData.Count
        AverageTimeTilFirstGrab              = ($logData | Measure-Object -Property TimeTilFirstGrab -Sum).Sum / $logData.Count
        AverageNumberOfGrabs                 = ($logData | Measure-Object -Property NumberOfGrabs -Sum).Sum / $logData.Count
        AverageNumberOfSnapsToFillables      = ($logData | Measure-Object -Property NumberOfSnapsToFillables -Sum).Sum / $logData.Count
        AverageSnapsPerGrabbable             = ($logData | ForEach-Object { if ($_.NumberOfGrabbables -ne 0) { $_.NumberOfSnapsToFillables / $_.NumberOfGrabbables } } | Measure-Object -Average).Average
        AverageNumberOfFillableTransparency  = ($logData | Measure-Object -Property NumberOfFillableTransparency -Sum).Sum / $logData.Count
        AverageNumberOfGrabbableTransparency = ($logData | Measure-Object -Property NumberOfGrabbableTransparency -Sum).Sum / $logData.Count
    }
    # Not adding TutorialStep, LevelID and LevelCompleted to the CSV, as they are already in the header or otherwise implied
    # NumberOfFillables and NumberOfGrabbables are not added to the CSV, as they can be cross-referenced and don't need to be added to every user line
    foreach ($line in $logData) {
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_TimeSinceStart"] = $line.TimeSinceStart
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_TimeToComplete"] = $line.TimeToComplete
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_TimeTilFirstGrab"] = $line.TimeTilFirstGrab
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_NumberOfGrabs"] = $line.NumberOfGrabs
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_NumberOfSnapsToFillables"] = $line.NumberOfSnapsToFillables
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_NumberOfFillableTransparency"] = $line.NumberOfFillableTransparency
        $csvLine["$($line.TutorialStep)_$($line.LevelID)_NumberOfGrabbableTransparency"] = $line.NumberOfGrabbableTransparency
    }
    $csvData += New-Object PSObject -Property $csvLine
}
# Changes back every . to a comma
$csvData | ForEach-Object {
    foreach ($property in $_.PSObject.Properties) {
        if ($property.Value -match '^\d+\.\d+$') {
            $property.Value = $property.Value -replace '\.', ','
        }
    }
}
# Ensure all possible columns are included by collecting all property names
$allColumns = @()
foreach ($entry in $csvData) {
    foreach ($name in $entry.PSObject.Properties.Name) {
        if (-not $allColumns.Contains($name)) {
            $allColumns += $name
        }
    }
}

# Export with all columns, filling missing values with empty strings
$csvData | Select-Object $allColumns | Export-Csv -Path $targetPath -NoTypeInformation -Force


# Hinzufügen:
# Welches Level vor Zeitlimit gestartet wurde
# Welches letztes geschafftes Level war
