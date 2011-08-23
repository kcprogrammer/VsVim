﻿
namespace Vim
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Operations
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Outlining

type internal CommandUtil =

    new : VimBufferData * IMotionUtil * ICommonOperations * ISmartIndentationService * IFoldManager * IInsertUtil -> CommandUtil

    interface ICommandUtil

