package main

import (
	"image"
	"image/color"
	"image/png"
	"log"
	"math"
	"os"
)

func main() {
	// -- config --
	h := 256
	p := 0.6
	path := "../../Assets/World/Dream/Dream_Gradient.png"

	// -- program --
	img := image.NewRGBA(image.Rectangle{
		image.Point{0, 0},
		image.Point{1, h},
	})

	// val := 1.0
	col := color.RGBA{0, 0, 0, 0xff}

	for y := 0; y < h; y++ {
		val := 1 - math.Pow(float64(y) / 255.0, p);
		cmp := uint8(math.Floor(val * 255.0))

		col.R = cmp
		col.G = cmp
		col.B = cmp

		img.Set(0, y, col)
	}

	f, _ := os.Create(path)
	png.Encode(f, img)
}
