﻿
#light

namespace Vim

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Operations
open Microsoft.VisualStudio.Utilities

// TODO: Need to add a Close method and do actions like close the ITrackingVisualSpan
type internal VimTextBuffer 
    (
        _textBuffer : ITextBuffer,
        _localSettings : IVimLocalSettings,
        _jumpList : IJumpList,
        _wordNavigator : ITextStructureNavigator,
        _bufferTrackingService : IBufferTrackingService,
        _vim : IVim
    ) =

    let _vimHost = _vim.VimHost
    let _globalSettings = _localSettings.GlobalSettings
    let _switchedModeEvent = new Event<_>()
    let mutable _modeKind = ModeKind.Normal
    let mutable _lastVisualSelection : ITrackingVisualSelection option = None

    member x.LastVisualSelection 
        with get() =
            match _lastVisualSelection with
            | None -> None
            | Some trackingVisualSelection -> trackingVisualSelection.VisualSelection
        and set value = 

            // First clear out the previous information
            match _lastVisualSelection with
            | None -> ()
            | Some trackingVisualSelection -> trackingVisualSelection.Close()

            match value with
            | None -> ()
            | Some visualSelection -> _lastVisualSelection <- Some (_bufferTrackingService.CreateVisualSelection visualSelection)

    /// Get all of the local marks in the IVimTextBuffer.
    member x.LocalMarks = 
        LocalMark.All
        |> Seq.map (fun localMark ->
            match x.GetLocalMark localMark with
            | None -> None
            | Some point -> Some (localMark, point))
        |> SeqUtil.filterToSome

    /// Get the specified local mark value
    member x.GetLocalMark localMark =
        match localMark with
        | LocalMark.Letter letter ->
            let found, trackingLineColumn = _textBuffer.Properties.TryGetProperty<ITrackingLineColumn>(letter)
            if found then
                trackingLineColumn.VirtualPoint
            else
                None
        | LocalMark.LastSelectionStart ->
            x.LastVisualSelection 
            |> Option.map (fun visualSelection -> visualSelection.VisualSpan.Start |> VirtualSnapshotPointUtil.OfPoint) 
        | LocalMark.LastSelectionEnd ->
            x.LastVisualSelection
            |> Option.map (fun visualSelection -> visualSelection.VisualSpan.End |> VirtualSnapshotPointUtil.OfPoint) 

    /// Set the local mark at the given line and column
    member x.SetLocalMark localMark line column = 
        match localMark with
        | LocalMark.Letter letter -> 
            // First close out the existing mark at this location if it exists
            let found, trackingLineColumn = _textBuffer.Properties.TryGetProperty<ITrackingLineColumn>(letter)
            if found then
                trackingLineColumn.Close()

            let trackingLineColumn = _bufferTrackingService.CreateLineColumn _textBuffer line column LineColumnTrackingMode.Default
            _textBuffer.Properties.[letter] <- trackingLineColumn
        | LocalMark.LastSelectionEnd ->
            ()
        | LocalMark.LastSelectionStart ->
            ()

    /// Switch to the desired mode
    member x.SwitchMode modeKind modeArgument =
        _modeKind <- modeKind
        _switchedModeEvent.Trigger (modeKind, modeArgument)

    interface IVimTextBuffer with
        member x.TextBuffer = _textBuffer
        member x.GlobalSettings = _globalSettings
        member x.JumpList = _jumpList
        member x.LastVisualSelection 
            with get () = x.LastVisualSelection
            and set value = x.LastVisualSelection <- value
        member x.LocalMarks = x.LocalMarks
        member x.LocalSettings = _localSettings
        member x.ModeKind = _modeKind
        member x.Name = _vimHost.GetName _textBuffer
        member x.Vim = _vim
        member x.WordNavigator = _wordNavigator
        member x.GetLocalMark localMark = x.GetLocalMark localMark
        member x.SetLocalMark localMark line column = x.SetLocalMark localMark line column
        member x.SwitchMode modeKind modeArgument = x.SwitchMode modeKind modeArgument

        [<CLIEvent>]
        member x.SwitchedMode = _switchedModeEvent.Publish

