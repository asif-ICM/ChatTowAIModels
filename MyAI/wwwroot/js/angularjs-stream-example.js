/**
 * AngularJS Example: ChatBot Stream Endpoint Integration
 * 
 * This is a complete example showing how to call the StreamChat endpoint
 * using AngularJS and handle Server-Sent Events (SSE) stream.
 */

// Step 1: Create AngularJS Module
var app = angular.module('chatBotApp', []);

// Step 2: Create Service for Streaming
app.service('ChatBotStreamService', ['$http', function($http) {
    this.streamChat = function(message, onChunk, onComplete, onError) {
        // Use fetch API for SSE support (modern browsers)
        fetch('/api/ChatBot/stream', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Message: message })
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
                        if (onComplete) onComplete();
                        return;
                    }

                    // Decode chunk
                    buffer += decoder.decode(result.value, { stream: true });
                    var lines = buffer.split('\n');
                    buffer = lines.pop() || '';

                    // Process each line
                    lines.forEach(function(line) {
                        if (line.startsWith('data: ')) {
                            var data = line.slice(6).trim();
                            
                            if (data === '[DONE]') {
                                if (onComplete) onComplete();
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

    return this;
}]);

// Step 3: Create Controller
app.controller('ChatBotController', ['$scope', 'ChatBotStreamService', function($scope, ChatBotStreamService) {
    var vm = this;
    
    vm.messages = [];
    vm.inputMessage = '';
    vm.isStreaming = false;
    vm.currentStreamingMessage = null;

    // Add message helper
    vm.addMessage = function(content, isUser) {
        var message = {
            content: content,
            isUser: isUser || false,
            timestamp: new Date()
        };
        vm.messages.push(message);
        return message;
    };

    // Stream chat function
    vm.streamChat = function() {
        if (!vm.inputMessage || vm.inputMessage.trim() === '') {
            return;
        }

        // Add user message
        vm.addMessage(vm.inputMessage, true);
        
        var message = vm.inputMessage;
        vm.inputMessage = '';
        vm.isStreaming = true;

        // Create streaming message placeholder
        vm.currentStreamingMessage = vm.addMessage('', false);

        // Call streaming service
        ChatBotStreamService.streamChat(
            message,
            // onChunk - called for each chunk
            function(content, timestamp) {
                if (vm.currentStreamingMessage) {
                    vm.currentStreamingMessage.content += content;
                } else {
                    vm.currentStreamingMessage = vm.addMessage(content, false);
                }
                $scope.$apply(); // Update AngularJS scope
            },
            // onComplete - called when stream completes
            function() {
                vm.isStreaming = false;
                vm.currentStreamingMessage = null;
                $scope.$apply();
            },
            // onError - called on error
            function(error) {
                console.error('Stream error:', error);
                vm.addMessage('Error: ' + error.message, false);
                vm.isStreaming = false;
                vm.currentStreamingMessage = null;
                $scope.$apply();
            }
        );
    };

    // Initialize with welcome message
    vm.addMessage('Hello! Ask me anything and I\'ll respond with live streaming!', false);
}]);

/**
 * Usage Example in HTML:
 * 
 * <div ng-app="chatBotApp" ng-controller="ChatBotController as vm">
 *     <div ng-repeat="message in vm.messages">
 *         <div class="{{message.isUser ? 'user' : 'bot'}}">
 *             {{message.content}}
 *         </div>
 *     </div>
 *     
 *     <input ng-model="vm.inputMessage" 
 *            ng-keypress="$event.key === 'Enter' && vm.streamChat()">
 *     <button ng-click="vm.streamChat()" 
 *             ng-disabled="vm.isStreaming">
 *         Stream Chat
 *     </button>
 * </div>
 */

