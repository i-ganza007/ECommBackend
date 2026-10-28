param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$UserId = "40270CBA-9133-439E-B7DE-5AD76110FC59",
    [string[]]$ProductIds = @("2B1C3495-435C-4A48-B128-797EB74FACF4"),
    [int]$OrderCount = 50,
    [int]$Concurrency = 5
)

if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw "Run this script with PowerShell 7 or later."
}

if ($OrderCount -lt 1 -or $Concurrency -lt 1) {
    throw "OrderCount and Concurrency must be greater than zero."
}

$requests = for ($i = 1; $i -le $OrderCount; $i++) {
    [pscustomobject]@{
        Number = $i
        Url = "$($BaseUrl.TrimEnd('/'))/api/orders"
        UserId = $UserId
        ProductIds = $ProductIds
    }
}

$results = $requests | ForEach-Object -Parallel {
    $request = $_
    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $body = @{
        _TotalPrice = 0
        _OrderCreatorId = $request.UserId
        _ProductsIds = $request.ProductIds
        _CreatedDate = [DateTime]::UtcNow.ToString("O")
        _OrderStatus = 0
    } | ConvertTo-Json -Depth 4

    try {
        $response = Invoke-RestMethod `
            -Uri $request.Url `
            -Method Post `
            -Headers @{ loggedInUserId = $request.UserId } `
            -ContentType "application/json" `
            -Body $body `
            -StatusCodeVariable statusCode

        $timer.Stop()
        [pscustomobject]@{
            Number = $request.Number
            Success = ($statusCode -eq 201)
            StatusCode = $statusCode
            OrderId = $response
            DurationMs = $timer.ElapsedMilliseconds
            Error = $null
        }
    }
    catch {
        $timer.Stop()
        $statusCode = 0
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        [pscustomobject]@{
            Number = $request.Number
            Success = $false
            StatusCode = $statusCode
            OrderId = $null
            DurationMs = $timer.ElapsedMilliseconds
            Error = $_.Exception.Message
        }
    }
} -ThrottleLimit $Concurrency

$succeeded = @($results | Where-Object Success)
$failed = @($results | Where-Object { -not $_.Success })

$results | Sort-Object Number | Format-Table Number, Success, StatusCode, OrderId, DurationMs -AutoSize
Write-Host "Succeeded: $($succeeded.Count) / $OrderCount; failed: $($failed.Count); concurrency: $Concurrency"

if ($failed.Count -gt 0) {
    $failed | Select-Object Number, StatusCode, Error | Format-Table -Wrap -AutoSize
    exit 1
}
