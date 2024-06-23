#!/bin/sh

# -- colors --
# TODO: consider updating to be more like:
# https://stackoverflow.com/questions/16843382/colored-shell-script-output-library
nc="\033[0m"
wh="\033[1;37m"
ge="\033[0;32m"
rd="\033[0;31m"
ga="\033[0;90m"

# -- styles --
uga="\033[90;4m"

# -- basic --
# print
pp() {
  printf "%b${nc}" "$1"
}

# print line
pl() {
  printf "${2}%b${nc}\n" "$1"
}

# -- elements --
# print title
pt() {
  printf "${uga}%b${nc}\n" "$1"
}

# print simple usage
pu() {
  printf "${uga}usage:${nc} $(basename "$0") %b${nc}\n" "$1" 1>&2; exit 1
}

# print info
pi() {
  printf "${wh}- ${nc}%b${nc}\n" "$1"
}

# print error
pe() {
  printf "${rd}✗ ${nc}%b${nc}\n" "$1"
}

# print fatal error (quit)
pf() {
  printf "${rd}✗ ${nc}%b${nc}\n" "$2"
  exit "$1"
}

# print success
ps() {
  printf "${ge}✓ ${nc}%b${nc}\n" "$1"
}

# -- style --
sr() {
  printf "%b" "${nc}"
}

sw() {
  printf "%b" "${wh}"
}

sg() {
  printf "%b" "${ga}"
}
