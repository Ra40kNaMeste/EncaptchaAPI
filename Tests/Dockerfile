From  mcr.microsoft.com/playwright:v1.47.2-focal
# copy project (including tests)
COPY . /e2e

WORKDIR /e2e

# Install dependencies
RUN npm install
# Install browsers
RUN npx -y playwright@1.47.2 install --with-deps

# Run playwright test
CMD [ "npx", "playwright", "test", "--reporter=list" ]