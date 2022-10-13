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
  local variant_dir="$BUILDS_DIR/$1"

  # validate variant dir
  if [ ! -d "$variant_dir" ]; then
    echo "✘ no variant dir @ '$variant_dir'"
    exit 100
  fi

  # find most recent build for the variant
  local build_dir=$(\
    ls -A $variant_dir \
      | sort -r -n -t \. -k 2 \
      | head -1
  )

  # if missing, the variant dir was empty
  if [ -z "$build_dir" ]; then
    echo "✘ no builds @ '$variant_dir'"
    exit 101
  fi

  # return the build
  build="$variant_dir/$build_dir"
}