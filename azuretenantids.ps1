$filepathDataInput = "C:\...\test.csv"

$filepathDataOutput = "C:\...\test-out.csv"

 

# Read input csv into object

$inputCsvList = Import-Csv $filepathDataInput

 

for ($counter=0; $counter -lt $inputCsvList.Length; $counter++)

{

    $entity = $inputCsvList[$counter]

    $username = $entity."Azure Username"

    $password = $entity."Azure Password"

    $securePassword = ConvertTo-SecureString $password -AsPlainText -Force

    $creds = New-Object System.Management.Automation.PSCredential ($username, $securePassword)

    $loggedIn = Login-AzureRmAccount -Credential $creds

    $subscription = Get-AzureRmSubscription
    $entity."Tenant Id" = $subscription.TenantId

    Write-Output "Finished $counter"

}

 

Write-Host "Writing to output file..."
$inputCsvList | ConvertTo-Csv -NoTypeInformation | % { 
        if ($ExcludeQuotesOutput) {
            $_ -replace '"',""
        } else {
            $_
        }
    } | Out-File -FilePath $filepathDataOutput -Append