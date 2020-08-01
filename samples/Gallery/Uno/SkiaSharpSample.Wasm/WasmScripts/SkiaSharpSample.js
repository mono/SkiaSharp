
function fileSaveAs(fileName, dataPtr, size) {
    // copy data into buffer
    var buffer = new Uint8Array(size);
    for (var i = 0; i < size; i++) {
        buffer[i] = Module.getValue(dataPtr + i, "i8");
    }

    // create the download anchor
    var a = window.document.createElement('a');
    var blob = new Blob([buffer]);
    a.href = window.URL.createObjectURL(blob);
    a.download = fileName;

    // append anchor to body
    document.body.appendChild(a);

    // "click" download
    a.click();

    // remove anchor from body
    document.body.removeChild(a); 
}
