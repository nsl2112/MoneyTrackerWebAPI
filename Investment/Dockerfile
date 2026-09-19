FROM python:3.14
WORKDIR /app
COPY ./requirements.txt /app/requirements.txt
RUN pip install --no-cache-dir -r /app/requirements.txt
COPY . /app/
CMD ["fastapi", "run", "src/main.py", "--port", "8081"]