import http.server
import socketserver

socketserver.TCPServer.allow_reuse_address = True

PORT = 8000

Handler = http.server.SimpleHTTPRequestHandler
Handler.extensions_map['.wasm'] = 'application/wasm'

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print("python 3 serving at port", PORT)
    httpd.serve_forever()