package main

import (
    "fmt"
	"bytes"
	"strconv"
	"io"
    "golang.org/x/net/http2"
    "golang.org/x/net/http2/h2c"
    "net"
    "net/http"
    "os"
)

func checkErr(err error, msg string) {
    if err == nil {
        return
    }
    fmt.Printf("ERROR: %s: %s\n", msg, err)
    os.Exit(1)
}

func main() {
    fmt.Printf("yo?")
    H2CServerUpgrade()
    //H2CServerPrior()
}

func get64KbString() string {
	var b bytes.Buffer

	for i := 0; i < 1000; i++ {
		b.WriteString("qwertzuiopasdfghjklyxcvbnm1234567890QWERTZUIOPASDFGHJKLYXCVBNM!!")
	}

	return b.String()
}

// This server supports "H2C upgrade" and "H2C prior knowledge" along with
// standard HTTP/2 and HTTP/1.1 that golang natively supports.
func H2CServerUpgrade() {
    h2s := &http2.Server{}

    var largeStr = get64KbString()

    handler := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		var lengthMbStr = r.URL.Query().Get("lengthMb")
		var lengthMb, err = strconv.Atoi(lengthMbStr)
		if err != nil {
			fmt.Printf("Atoi failed")
			return
		}
		
        //fmt.Fprintf(w, "Hello, %v, H2C: %v, lengthMb: %v", r.URL.Path, r.TLS == nil, lengthMb)

		fmt.Printf("Hello, %v, H2C: %v, lengthMb: %v", r.URL.Path, r.TLS == nil, lengthMb)

		for i := 0; i < lengthMb * 1024 / 64; i++ {
			io.WriteString(w, largeStr)
		}
    })

    server := &http.Server{
        Addr:    "0.0.0.0:1010",
        Handler: h2c.NewHandler(handler, h2s),
    }

    fmt.Printf("Listening ...\n")
    checkErr(server.ListenAndServe(), "while listening")
}

// This server only supports "H2C prior knowledge".
// You can add standard HTTP/2 support by adding a TLS config.
func H2CServerPrior() {
    server := http2.Server{}

    l, err := net.Listen("tcp", "0.0.0.0:1010")
    checkErr(err, "while listening")

    fmt.Printf("Listening [0.0.0.0:1010]...\n")
    for {
        conn, err := l.Accept()
        checkErr(err, "during accept")

        server.ServeConn(conn, &http2.ServeConnOpts{
            Handler: http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
                fmt.Printf("New Connection: %+v\n", r)
                fmt.Fprintf(w, "Hello, %v, http: %v", r.URL.Path, r.TLS == nil)
            }),
        })
    }
}