#include "mongoose.h"
#include <string.h>
#include <stdio.h>
#include <ctype.h>

// Save new user to file
int save_user(const char *username, const char *email, const char *password) {
    FILE *f = fopen("users.txt", "a");
    if (!f) return 0;
    fprintf(f, "%s,%s,%s\n", username, email, password);
    fclose(f);
    return 1;
}

// Check if username already exists
int is_username_taken(const char *username) {
    FILE *f = fopen("users.txt", "r");
    if (!f) return 0;

    char line[256], saved_username[100];
    while (fgets(line, sizeof(line), f)) {
        sscanf(line, "%[^,]", saved_username); // Read until first comma
        if (strcmp(saved_username, username) == 0) {
            fclose(f);
            return 1; // Username exists
        }
    }
    fclose(f);
    return 0;
}

// Check if username contains only letters
int is_valid_username(const char *username) {
    for (int i = 0; username[i] != '\0'; i++) {
        if (!isalpha(username[i])) return 0;
    }
    return 1;
}

void extract_json_field(struct mg_str json, const char *path, char *dest, size_t dest_size) {
    char *val = mg_json_get_str(json, path);
    if (val) {
        strncpy(dest, val, dest_size - 1);
        dest[dest_size - 1] = '\0';
        free(val);
    } else {
        strcpy(dest, "");
    }
}

static void fn(struct mg_connection *c, int ev, void *ev_data) {
    if (ev == MG_EV_HTTP_MSG) {
        struct mg_http_message *hm = (struct mg_http_message *) ev_data;

        if (mg_strcmp(hm->uri, mg_str("/register")) == 0) {
            char username[100], email[100], password[100];

            extract_json_field(hm->body, "$.username", username, sizeof(username));
            extract_json_field(hm->body, "$.email", email, sizeof(email));
            extract_json_field(hm->body, "$.password", password, sizeof(password));

            printf("Register request received:\nUsername: %s\nEmail: %s\nPassword: %s\n", username, email, password);

            // Perform validation
            if (!is_valid_username(username)) {
                mg_http_reply(c, 400, "Content-Type: application/json\r\n", "{ \"error\": \"Username must contain only letters\" }\n");
                return;
            }

            if (strlen(password) < 5) {
                mg_http_reply(c, 400, "Content-Type: application/json\r\n", "{ \"error\": \"Password must be at least 5 characters\" }\n");
                return;
            }

            if (is_username_taken(username)) {
                mg_http_reply(c, 400, "Content-Type: application/json\r\n", "{ \"error\": \"Username already exists\" }\n");
                return;
            }

            // Save new user
            if (!save_user(username, email, password)) {
                mg_http_reply(c, 500, "", "Server Error\n");
                return;
            }

            mg_http_reply(c, 200, "Content-Type: application/json\r\n", "{ \"message\": \"registration successful\" }\n");
        } else {
            mg_http_reply(c, 404, "", "Not Found\n");
        }
    }
}

int main(void) {
    struct mg_mgr mgr;
    mg_mgr_init(&mgr);

    if (mg_http_listen(&mgr, "http://0.0.0.0:8080", fn, NULL) == NULL) {
        fprintf(stderr, "Failed to start server\n");
        return 1;
    }

    printf("Server running at http://localhost:8080\n");

    for (;;) {
        mg_mgr_poll(&mgr, 1000);
    }

    mg_mgr_free(&mgr);
    return 0;
}
