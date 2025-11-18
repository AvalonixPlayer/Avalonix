{ pkgs ? import <nixpkgs> {} }:

let
  nativeDeps = with pkgs; [
    zlib
    icu
    openssl

    fontconfig
    freetype
    expat
    libpng
    libjpeg
    libGL
    libuuid

    glib
    gtk3

    xorg.libX11
    xorg.libXext
    xorg.libXrender
    xorg.libXfixes
    xorg.libXi
    xorg.libXcursor
    xorg.libXrandr
    xorg.libXinerama
    xorg.libXdamage
    xorg.libxcb
    xorg.libICE
    xorg.libSM

    libxkbcommon
  ];
in

pkgs.mkShell {
  buildInputs = [
    pkgs.dotnet-sdk_9
  ] ++ nativeDeps;

  LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath nativeDeps;
  FONTCONFIG_FILE = "${pkgs.fontconfig.out}/etc/fonts/fonts.conf";
}
