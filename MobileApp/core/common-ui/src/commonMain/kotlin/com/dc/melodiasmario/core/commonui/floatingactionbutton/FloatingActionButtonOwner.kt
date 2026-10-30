package com.dc.melodiasmario.core.commonui.floatingactionbutton

/**
 * Provides floating action button owner for the floatingactionbutton layer.
 */
interface FloatingActionButtonOwner

private class FloatingActionButtonOwnerImpl : FloatingActionButtonOwner

/**
 * Provides the floating action button owner operation for floating action button owner.
 */
fun floatingActionButtonOwner(): FloatingActionButtonOwner = FloatingActionButtonOwnerImpl()
