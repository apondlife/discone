#!/bin/sh

# -- constants --
# the directory containing build variants
BUILDS_DIR="./Artifacts/Builds"

# the release variant name
VARIANT_RELEASE="release"

# the playtest variant name
VARIANT_PLAYTEST="playtest"

# -- queries --
# find the most recent build for the variant; sets $build
FindBuild() {
  variant_dir="$BUILDS_DIR/$1"

  # validate variant dir
  if [ ! -d "$variant_dir" ]; then
    pf 100 "no variant dir @ '$variant_dir'"
  fi

  # find most recent build for the variant
  build_dir=$(\
    find Artifacts/Builds/release -depth 1 -name '*discone*' \
      | sort -r \
      | head -1
  )

  # if missing, the variant dir was empty
  if [ -z "$build_dir" ]; then
    pf "no builds @ '$variant_dir'"
  fi

  # return the build
  BUILD="$build_dir"
}
