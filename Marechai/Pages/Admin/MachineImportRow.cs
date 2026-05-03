/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Admin;

public enum MachineFamilyMatchType
{
    Exact,
    Partial,
    New,
    None
}

public enum GpuMatchType
{
    Exact,
    Partial,
    None
}

public enum ProcessorMatchType
{
    Exact,
    Partial,
    None
}

public enum SoundSynthMatchType
{
    Exact,
    Partial,
    None
}

public enum SoftwarePlatformMatchType
{
    Exact,
    Partial,
    None
}

public enum MachineTypeMatchType
{
    Exact,
    Partial,
    None
}

public enum MachineNameDuplicateType
{
    ExactDuplicate,
    SoundexDuplicate,
    None
}

public sealed class MachineImportRow
{
    // CSV fields
    public string  Name         { get; set; } = "";
    public string? CompanyInput { get; set; }
    public string? TypeInput    { get; set; }
    public string? Model        { get; set; }
    public string? FamilyInput  { get; set; }
    public int?    IntroducedYear  { get; set; }
    public int?    IntroducedMonth { get; set; }
    public int?    IntroducedDay   { get; set; }
    public string? GpuInput     { get; set; }
    public string? ProcessorInput    { get; set; }
    public double? ProcessorSpeed    { get; set; }
    public string? SoundSynthInput   { get; set; }
    public string? SoftwarePlatformInput { get; set; }

    // Memory fields
    public string? WorkMemoryTypeInput    { get; set; }
    public long?   WorkMemorySize         { get; set; }
    public double? WorkMemorySpeed        { get; set; }
    public string? VideoMemoryTypeInput   { get; set; }
    public long?   VideoMemorySize        { get; set; }
    public double? VideoMemorySpeed       { get; set; }
    public string? SoundMemoryTypeInput   { get; set; }
    public long?   SoundMemorySize        { get; set; }
    public double? SoundMemorySpeed       { get; set; }
    public string? UnifiedMemoryTypeInput { get; set; }
    public long?   UnifiedMemorySize      { get; set; }
    public double? UnifiedMemorySpeed     { get; set; }
    public string? FirmwareMemoryTypeInput { get; set; }
    public long?   FirmwareMemorySize     { get; set; }
    public double? FirmwareMemorySpeed    { get; set; }

    // Company match state
    public List<CompanyDto>  MatchedCompanies { get; set; } = [];
    public CompanyDto?       SelectedCompany  { get; set; }
    public CompanyMatchType  CompanyMatch     { get; set; } = CompanyMatchType.None;

    // Machine type match state
    public MachineTypeMatchType TypeMatch     { get; set; } = MachineTypeMatchType.None;
    public int?                 MatchedType   { get; set; }

    // Family match state
    public List<MachineFamilyDto> MatchedFamilies { get; set; } = [];
    public MachineFamilyDto?      SelectedFamily  { get; set; }
    public MachineFamilyMatchType FamilyMatch     { get; set; } = MachineFamilyMatchType.None;

    // GPU match state
    public List<GpuDto>  MatchedGpus { get; set; } = [];
    public GpuDto?       SelectedGpu { get; set; }
    public GpuMatchType  GpuMatch    { get; set; } = GpuMatchType.None;

    // Processor match state
    public List<ProcessorDto>  MatchedProcessors { get; set; } = [];
    public ProcessorDto?       SelectedProcessor { get; set; }
    public ProcessorMatchType  ProcessorMatch    { get; set; } = ProcessorMatchType.None;

    // Sound synth match state
    public List<SoundSynthDto>  MatchedSoundSynths { get; set; } = [];
    public SoundSynthDto?       SelectedSoundSynth { get; set; }
    public SoundSynthMatchType  SoundSynthMatch    { get; set; } = SoundSynthMatchType.None;

    // Software platform match state
    public List<SoftwarePlatformDto>  MatchedSoftwarePlatforms { get; set; } = [];
    public SoftwarePlatformDto?       SelectedSoftwarePlatform { get; set; }
    public SoftwarePlatformMatchType  SoftwarePlatformMatch    { get; set; } = SoftwarePlatformMatchType.None;

    // Memory type validation
    public bool WorkMemoryTypeValid     { get; set; } = true;
    public int  WorkMemoryTypeValue     { get; set; }
    public bool VideoMemoryTypeValid    { get; set; } = true;
    public int  VideoMemoryTypeValue    { get; set; }
    public bool SoundMemoryTypeValid    { get; set; } = true;
    public int  SoundMemoryTypeValue    { get; set; }
    public bool UnifiedMemoryTypeValid  { get; set; } = true;
    public int  UnifiedMemoryTypeValue  { get; set; }
    public bool FirmwareMemoryTypeValid { get; set; } = true;
    public int  FirmwareMemoryTypeValue { get; set; }

    // Duplicate detection
    public MachineNameDuplicateType DuplicateType       { get; set; } = MachineNameDuplicateType.None;
    public string?                  DuplicateMatchedName { get; set; }

    // Import state
    public string? ImportError     { get; set; }
    public string? ValidationError { get; set; }

    public bool IsPrototype => IntroducedYear is null && IntroducedMonth is null && IntroducedDay is null;

    public DateTime? IntroducedDate
    {
        get
        {
            if(IntroducedYear is null)
                return null;

            int month = IntroducedMonth ?? 1;
            int day   = IntroducedDay   ?? 1;

            try
            {
                return new DateTime(IntroducedYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int IntroducedPrecision
    {
        get
        {
            if(IntroducedYear is null)
                return 0;

            if(IntroducedMonth is null)
                return 2;

            if(IntroducedDay is null)
                return 1;

            return 0;
        }
    }

    public string CompanyDisplay
    {
        get
        {
            if(CompanyMatch is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedCompany is not null)
                return SelectedCompany.Name ?? CompanyInput ?? "";

            return CompanyInput ?? "";
        }
    }

    public string TypeDisplay => MatchedType switch
    {
        1 => "Computer",
        2 => "Console",
        3 => "Smartphone",
        _ => TypeInput ?? ""
    };

    public string FamilyDisplay
    {
        get
        {
            if(FamilyMatch is MachineFamilyMatchType.Exact or MachineFamilyMatchType.Partial &&
               SelectedFamily is not null)
                return SelectedFamily.Name ?? FamilyInput ?? "";

            if(FamilyMatch == MachineFamilyMatchType.New)
                return FamilyInput ?? "";

            return FamilyInput ?? "";
        }
    }

    public string GpuDisplay
    {
        get
        {
            if(GpuMatch is GpuMatchType.Exact or GpuMatchType.Partial && SelectedGpu is not null)
                return SelectedGpu.Name ?? GpuInput ?? "";

            return GpuInput ?? "";
        }
    }

    public string ProcessorDisplay
    {
        get
        {
            if(ProcessorMatch is ProcessorMatchType.Exact or ProcessorMatchType.Partial &&
               SelectedProcessor is not null)
                return SelectedProcessor.Name ?? ProcessorInput ?? "";

            return ProcessorInput ?? "";
        }
    }

    public string SoundSynthDisplay
    {
        get
        {
            if(SoundSynthMatch is SoundSynthMatchType.Exact or SoundSynthMatchType.Partial &&
               SelectedSoundSynth is not null)
                return SelectedSoundSynth.Name ?? SoundSynthInput ?? "";

            return SoundSynthInput ?? "";
        }
    }

    public string SoftwarePlatformDisplay
    {
        get
        {
            if(SoftwarePlatformMatch is SoftwarePlatformMatchType.Exact or SoftwarePlatformMatchType.Partial &&
               SelectedSoftwarePlatform is not null)
                return SelectedSoftwarePlatform.Name ?? SoftwarePlatformInput ?? "";

            return SoftwarePlatformInput ?? "";
        }
    }

    public bool HasWorkMemory     => WorkMemorySize is > 0 || !string.IsNullOrWhiteSpace(WorkMemoryTypeInput);
    public bool HasVideoMemory    => VideoMemorySize is > 0 || !string.IsNullOrWhiteSpace(VideoMemoryTypeInput);
    public bool HasSoundMemory    => SoundMemorySize is > 0 || !string.IsNullOrWhiteSpace(SoundMemoryTypeInput);
    public bool HasUnifiedMemory  => UnifiedMemorySize is > 0 || !string.IsNullOrWhiteSpace(UnifiedMemoryTypeInput);
    public bool HasFirmwareMemory => FirmwareMemorySize is > 0 || !string.IsNullOrWhiteSpace(FirmwareMemoryTypeInput);
}
