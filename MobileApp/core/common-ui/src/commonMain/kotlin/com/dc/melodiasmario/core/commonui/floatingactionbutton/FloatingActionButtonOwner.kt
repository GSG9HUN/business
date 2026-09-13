package com.dc.melodiasmario.core.commonui.floatingactionbutton

interface FloatingActionButtonOwner

private class FloatingActionButtonOwnerImpl : FloatingActionButtonOwner

fun floatingActionButtonOwner(): FloatingActionButtonOwner = FloatingActionButtonOwnerImpl()