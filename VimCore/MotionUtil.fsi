﻿#light

namespace Vim

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Operations

type internal MotionUtil =
    new : VimBufferData -> MotionUtil

    interface IMotionUtil
