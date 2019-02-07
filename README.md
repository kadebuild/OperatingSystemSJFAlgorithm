# OperatingSystemSJFAlgorithm

Simple console realisation of SJF algorithm (Shortest Job First) on C# language as the final work in the discipline operating systems.

This code realize two variation of SJF:
- Preemptive planning for remaining execution time
- Non preemptive planning with execution prehistory

Each process has a number of parameters:
- V - Consumption of RAM
- H - Consumption of HDD
- N - Number of tacts that process need to work before sleep
- Working time - Number of tacts that process need to work before finish
- Arrival time - In which tact process came in system

The system as well as the process has several parameters (resources):
- V - Amount of RAM in system
- H - Amount of HDD storage in system

System create a list of ready to run process based on free amount of RAM and HDD.
