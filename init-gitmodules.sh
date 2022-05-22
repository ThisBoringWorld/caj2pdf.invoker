rm -rf ./src/PyPDF2
rm -rf ./src/caj2pdf

git submodule init
git submodule update

cd ./src/PyPDF2
git reset --hard 1.27.12

git submodule status 