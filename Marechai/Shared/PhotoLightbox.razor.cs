using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Marechai.Shared;

public partial class PhotoLightbox
{
    List<Guid> _photoIds = new();
    int        _currentIndex;

    [Parameter]
    public string BaseUrl { get; set; }

    [Parameter]
    public string Category { get; set; } = "machines";

    public bool Visible { get; private set; }

    Guid CurrentPhotoId => _photoIds.Count > 0 ? _photoIds[_currentIndex] : Guid.Empty;

    public void Show(List<Guid> photoIds, Guid selectedId)
    {
        _photoIds    = photoIds ?? new List<Guid>();
        _currentIndex = _photoIds.IndexOf(selectedId);

        if(_currentIndex < 0) _currentIndex = 0;

        Visible = true;
        StateHasChanged();
    }

    void Close()
    {
        Visible = false;
        StateHasChanged();
    }

    void Previous()
    {
        if(_photoIds.Count == 0) return;

        _currentIndex = (_currentIndex - 1 + _photoIds.Count) % _photoIds.Count;
        StateHasChanged();
    }

    void Next()
    {
        if(_photoIds.Count == 0) return;

        _currentIndex = (_currentIndex + 1) % _photoIds.Count;
        StateHasChanged();
    }
}
