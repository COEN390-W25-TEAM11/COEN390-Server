#include "mongoose.h"
#include <string.h>

// Helper function to safely extract string from JSON
void extract_json_field(struct mg_str json, const char *json_path, char *dest, size_t dest_size) {
    char *val = mg_json_get_str(json, json_path);
    if (val) {
        strncpy(dest, val, dest_size - 1);
        dest[dest_size - 1] = '\0';  // Ensure null termination
        free(val);  // mg_json_get_str allocates memory, so free it!
    } else {
        strcpy(dest, "N/A");
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

            printf("Register request received:\n");
            printf("Username: %s\n", username);
            printf("Email:    %s\n", email);
            printf("Password: %s\n", password);

            mg_http_reply(c, 200, "Content-Type: application/json\r\n",
                          "{ \"message\": \"registration successful\" }\n");
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
