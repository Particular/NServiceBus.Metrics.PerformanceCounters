
#requires -RunAsAdministrator
Function InstallNSBPerfCounters {

    $category = @{Name="NServiceBus"; Description="NServiceBus statistics"}
    $counters = New-Object System.Diagnostics.CounterCreationDataCollection
    $counters.AddRange(@(
        New-Object System.Diagnostics.CounterCreationData "SLA violation countdown", "Seconds until the SLA for this endpoint is breached.",  NumberOfItems32
        New-Object System.Diagnostics.CounterCreationData "Avg. Critical Time (sec)", "The time it took from sending to processing the message.",  AverageTimer32
       New-Object System.Diagnostics.CounterCreationData "Avg. Critical Time (sec)Base", "The time it took from sending to processing the message.",  AverageBase
        New-Object System.Diagnostics.CounterCreationData "Critical Time", "The time it took from sending to processing the message.",  NumberOfItems32
        New-Object System.Diagnostics.CounterCreationData "Avg. Processing Time (sec)", "The time it took to successfully process a message.",  AverageTimer32
       New-Object System.Diagnostics.CounterCreationData "Avg. Processing Time (sec)Base", "The time it took to successfully process a message.",  AverageBase
        New-Object System.Diagnostics.CounterCreationData "Processing Time", "The time it took to successfully process a message.",  NumberOfItems32
        New-Object System.Diagnostics.CounterCreationData "# of msgs failures / sec", "The current number of failed processed messages by the transport per second.",  RateOfCountsPerSecond32
        New-Object System.Diagnostics.CounterCreationData "# of msgs successfully processed / sec", "The current number of messages processed successfully by the transport per second.",  RateOfCountsPerSecond32
        New-Object System.Diagnostics.CounterCreationData "# of msgs pulled from the input queue /sec", "The current number of messages pulled from the input queue by the transport per second.",  RateOfCountsPerSecond32

    ))
    if ([System.Diagnostics.PerformanceCounterCategory]::Exists($category.Name)) {
        [System.Diagnostics.PerformanceCounterCategory]::Delete($category.Name)
    }
    [void] [System.Diagnostics.PerformanceCounterCategory]::Create($category.Name, $category.Description, [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance, $counters)
    [System.Diagnostics.PerformanceCounter]::CloseSharedResources()
}
InstallNSBPerfCounters
