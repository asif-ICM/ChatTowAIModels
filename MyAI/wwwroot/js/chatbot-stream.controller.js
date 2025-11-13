// AngularJS Controller for ChatBot Streaming
(function() {
    'use strict';

    angular.module('chatBotApp')
        .controller('ChatBotStreamController', ['$scope', 'ChatBotStreamService', function($scope, ChatBotStreamService) {
            var vm = this;

            // Initialize variables
            vm.messages = [];
            vm.currentMessage = '';
            vm.inputMessage = '';
            vm.isStreaming = false;
            vm.streamingContent = '';
            vm.currentStreamingMessage = null;

            /**
             * Add a message to the chat
             */
            vm.addMessage = function(content, isUser, messageType) {
                var message = {
                    content: content,
                    isUser: isUser || false,
                    messageType: messageType || 'normal',
                    timestamp: new Date()
                };
                vm.messages.push(message);
                return message;
            };

            /**
             * Update the current streaming message
             */
            vm.updateStreamingMessage = function(content) {
                if (vm.currentStreamingMessage) {
                    vm.currentStreamingMessage.content += content;
                } else {
                    vm.currentStreamingMessage = vm.addMessage(content, false, 'streaming');
                }
                
                // Trigger digest cycle
                $scope.$apply();
            };

            /**
             * Finalize streaming
             */
            vm.finalizeStreaming = function() {
                vm.currentStreamingMessage = null;
                vm.isStreaming = false;
                vm.streamingContent = '';
            };

            /**
             * Stream chat using SSE
             */
            vm.streamChat = function() {
                if (!vm.inputMessage || vm.inputMessage.trim() === '') {
                    return;
                }

                // Add user message
                vm.addMessage(vm.inputMessage, true);
                
                var message = vm.inputMessage;
                vm.inputMessage = '';
                vm.isStreaming = true;
                vm.streamingContent = '';

                // Create streaming message placeholder
                vm.currentStreamingMessage = vm.addMessage('', false, 'streaming');

                // Call the streaming service
                ChatBotStreamService.streamChat(
                    message,
                    // onChunk callback
                    function(content, timestamp) {
                        vm.updateStreamingMessage(content);
                    },
                    // onComplete callback
                    function() {
                        vm.finalizeStreaming();
                        $scope.$apply();
                    },
                    // onError callback
                    function(error) {
                        console.error('Stream error:', error);
                        vm.addMessage('Error: ' + error.message, false, 'error');
                        vm.finalizeStreaming();
                        $scope.$apply();
                    }
                );
            };

            /**
             * Think and respond
             */
            vm.thinkAndRespond = function() {
                if (!vm.inputMessage || vm.inputMessage.trim() === '') {
                    return;
                }

                // Add user message
                vm.addMessage(vm.inputMessage, true);
                
                var message = vm.inputMessage;
                vm.inputMessage = '';
                vm.isStreaming = true;

                // Call the thinking service
                ChatBotStreamService.thinkAndRespond(
                    message,
                    // onChunk callback
                    function(content, type, timestamp) {
                        if (type === 'thinking' || type === 'analyzing' || type === 'todo' || type === 'safety_warning') {
                            vm.addMessage(content, false, type);
                        } else if (type === 'response_start') {
                            vm.currentStreamingMessage = vm.addMessage(content, false, 'response-start');
                        } else if (type === 'response') {
                            vm.updateStreamingMessage(content);
                        } else if (type === 'complete') {
                            vm.finalizeStreaming();
                            vm.addMessage(content, false, 'complete');
                        }
                        $scope.$apply();
                    },
                    // onComplete callback
                    function() {
                        vm.finalizeStreaming();
                        $scope.$apply();
                    },
                    // onError callback
                    function(error) {
                        console.error('Think error:', error);
                        vm.addMessage('Error: ' + error.message, false, 'error');
                        vm.finalizeStreaming();
                        $scope.$apply();
                    }
                );
            };

            /**
             * Regular chat (non-streaming)
             */
            vm.sendChat = function() {
                if (!vm.inputMessage || vm.inputMessage.trim() === '') {
                    return;
                }

                // Add user message
                vm.addMessage(vm.inputMessage, true);
                
                var message = vm.inputMessage;
                vm.inputMessage = '';
                vm.isStreaming = true;

                ChatBotStreamService.chat(message)
                    .then(function(response) {
                        if (response.data && response.data.length > 0) {
                            // Display the last response
                            var lastResponse = response.data[response.data.length - 1];
                            if (!lastResponse.isUserMessage) {
                                vm.addMessage(lastResponse.message, false);
                            }
                        }
                        vm.isStreaming = false;
                    })
                    .catch(function(error) {
                        console.error('Chat error:', error);
                        vm.addMessage('Error: ' + (error.data || error.message), false, 'error');
                        vm.isStreaming = false;
                    });
            };

            /**
             * Handle Enter key press
             */
            vm.handleKeyPress = function(event) {
                if (event.key === 'Enter' && !event.shiftKey) {
                    event.preventDefault();
                    vm.streamChat();
                }
            };

            // Initialize with welcome message
            vm.addMessage('Hello! I\'m your AI assistant. Ask me anything and I\'ll respond with live streaming!', false);
        }]);
})();

