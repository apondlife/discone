#!/bin/sh

# -- parsing --
Require() {
  if [ -z "$1" ]; then
    Usage
  fi
}

RequireExists() {
  Require "$1"

  if [ ! -f "$1" ]; then
    pf "$1 does not exist."
  fi
}