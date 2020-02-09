# Contributing

## Commit signature
For security reason we require all commits to be cryptographically signed.
This section explains how to setup the development environment for that purpose.

### Visual Studio and Visual Studio Code for Windows
You need to install Git for Windows. It is available as a component of Visual Studio, or separately in https://gitforwindows.org.

You also need to install Gpg4win from https://www.gpg4win.org. Ensure to select the Kleopatra component.

Once you have them installed, open Kleopatra and generate a new key pair, of OpenPGP type, following the instructions [here](https://www.gpg4win.org/doc/en/gpg4win-compendium_12.html).
Save aside the fingerprint, you'll need it later.

Now go to environment variables (in the properties of your computer) and add this to the path:
`C:\Program Files\Git\usr\bin`

Finally, open Git Bash, and write the following commands if you want all git commits to be signed:
```bash
git config --global commit.gpgsign true
git config --global user.signingkey <FINGERPRINT>
git config --global gpg.program "C:\Program Files (x86)\GnuPG\bin\gpg.exe"
```
or if you want the options to apply only for this project
```bash
cd /DRIVE/PATH_TO_PROJECT
git config commit.gpgsign true
git config user.signingkey FINGERPRINT
git config gpg.program "C:\Program Files (x86)\GnuPG\bin\gpg.exe"
```

replacing `FINGERPRINT` with the fingerprint you saved from the key generation, `DRIVE` with the drive letter and `PATH_TO_PROJECT` using `/` as path separator.

Once this is done, every time you commit in VS / VSCode, a message box titled `pinentry-qt` will ask for the passphrase you set up earlier and sign the commit with your key.

For GitHub to recognize your signature you need to follow the steps [here](https://help.github.com/en/github/authenticating-to-github/adding-a-new-gpg-key-to-your-github-account). 