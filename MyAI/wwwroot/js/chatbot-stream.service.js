// AngularJS Service for ChatBot Streaming
(function() {
    'use strict';

    angular.module('chatBotApp')
        .service('ChatBotStreamService', ['$http', '$q', function($http, $q) {
            var service = this;

            /**
             * Stream chat using Server-Sent Events (SSE)
             * @param {string} message - The user's message
             * @param {function} onChunk - Callback function called for each chunk
             * @param {function} onComplete - Callback function called when stream completes
             * @param {function} onError - Callback function called on error
             */
            service.streamChat = function(message, onChunk, onComplete, onError) {
                // Use fetch API for SSE support
                var url = '/api/ChatBot/stream';
                var payload = JSON.stringify({ Message: message });
                
                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: payload
                })
                .then(function(response) {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }

                    var reader = response.body.getReader();
                    var decoder = new TextDecoder();
                    var buffer = '';

                    function readStream() {
                        reader.read().then(function(result) {
                            if (result.done) {
                                if (onComplete) {
                                    onComplete();
                                }
                                return;
                            }

                            // Decode the chunk
                            buffer += decoder.decode(result.value, { stream: true });
                            var lines = buffer.split('\n');
                            buffer = lines.pop() || ''; // Keep incomplete line in buffer

                            // Process each line
                            lines.forEach(function(line) {
                                if (line.startsWith('data: ')) {
                                    var data = line.slice(6).trim();
                                    
                                    if (data === '[DONE]') {
                                        if (onComplete) {
                                            onComplete();
                                        }
                                        return;
                                    }

                                    try {
                                        var parsed = JSON.parse(data);
                                        if (parsed.content && onChunk) {
                                            onChunk(parsed.content, parsed.timestamp);
                                        }
                                    } catch (e) {
                                        console.error('Error parsing SSE data:', e);
                                    }
                                }
                            });

                            // Continue reading
                            readStream();
                        })
                        .catch(function(error) {
                            if (onError) {
                                onError(error);
                            } else {
                                console.error('Stream error:', error);
                            }
                        });
                    }

                    readStream();
                })
                .catch(function(error) {
                    if (onError) {
                        onError(error);
                    } else {
                        console.error('Fetch error:', error);
                    }
                });
            };

            /**
             * Alternative method using XMLHttpRequest for older browsers
             * @param {string} message - The user's message
             * @param {function} onChunk - Callback function called for each chunk
             * @param {function} onComplete - Callback function called when stream completes
             * @param {function} onError - Callback function called on error
             */
            service.streamChatXHR = function(message, onChunk, onComplete, onError) {
                var xhr = new XMLHttpRequest();
                var url = '/api/ChatBot/stream';
                var buffer = '';

                xhr.open('POST', url, true);
                xhr.setRequestHeader('Content-Type', 'application/json');

                xhr.onprogress = function() {
                    if (xhr.readyState === 3) { // Loading state
                        var text = xhr.responseText;
                        var newData = text.slice(buffer.length);
                        buffer = text;

                        var lines = newData.split('\n');
                        lines.forEach(function(line) {
                            if (line.startsWith('data: ')) {
                                var data = line.slice(6).trim();
                                
                                if (data === '[DONE]') {
                                    if (onComplete) {
                                        onComplete();
                                    }
                                    return;
                                }

                                try {
                                    var parsed = JSON.parse(data);
                                    if (parsed.content && onChunk) {
                                        onChunk(parsed.content, parsed.timestamp);
                                    }
                                } catch (e) {
                                    // Ignore parsing errors
                                }
                            }
                        });
                    }
                };

                xhr.onload = function() {
                    if (xhr.status === 200) {
                        if (onComplete) {
                            onComplete();
                        }
                    } else {
                        if (onError) {
                            onError(new Error('Request failed with status: ' + xhr.status));
                        }
                    }
                };

                xhr.onerror = function() {
                    if (onError) {
                        onError(new Error('Network error'));
                    }
                };

                xhr.send(JSON.stringify({ Message: message }));
            };

            /**
             * Regular chat endpoint (non-streaming)
             */
            service.chat = function(message) {
                return $http({
                    method: 'POST',
                    url: '/api/ChatBot/chat',
                    data: { Message: message },
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
            };

            /**
             * Think and respond endpoint (with thinking process)
             */
            service.thinkAndRespond = function(message, onChunk, onComplete, onError) {
                var url = '/api/ChatBot/think';
                var payload = JSON.stringify({ Message: message });
                
                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: payload
                })
                .then(function(response) {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }

                    var reader = response.body.getReader();
                    var decoder = new TextDecoder();
                    var buffer = '';

                    function readStream() {
                        reader.read().then(function(result) {
                            if (result.done) {
                                if (onComplete) {
                                    onComplete();
                                }
                                return;
                            }

                            buffer += decoder.decode(result.value, { stream: true });
                            var lines = buffer.split('\n');
                            buffer = lines.pop() || '';

                            lines.forEach(function(line) {
                                if (line.startsWith('data: ')) {
                                    var data = line.slice(6).trim();
                                    
                                    if (data === '[DONE]') {
                                        if (onComplete) {
                                            onComplete();
                                        }
                                        return;
                                    }

                                    try {
                                        var parsed = JSON.parse(data);
                                        if (parsed.content && onChunk) {
                                            onChunk(parsed.content, parsed.type, parsed.timestamp);
                                        }
                                    } catch (e) {
                                        console.error('Error parsing SSE data:', e);
                                    }
                                }
                            });

                            readStream();
                        })
                        .catch(function(error) {
                            if (onError) {
                                onError(error);
                            } else {
                                console.error('Stream error:', error);
                            }
                        });
                    }

                    readStream();
                })
                .catch(function(error) {
                    if (onError) {
                        onError(error);
                    } else {
                        console.error('Fetch error:', error);
                    }
                });
            };

            return service;
        }]);
})();

